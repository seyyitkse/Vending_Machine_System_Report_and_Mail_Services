using Services.ApiLayer.Vending.EntityLayer.Context;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Services.ApiLayer.Services_Balance_
{
    public class CriticalAlertsService : BackgroundService
    {
        private readonly ILogger<CriticalAlertsService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly int _stockThreshold = 10;    // Kritik stok eşiği
        private readonly decimal _balanceThreshold = 5.0m; // Kritik bakiye eşiği

        public CriticalAlertsService(ILogger<CriticalAlertsService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var targetTime = DateTime.Today.AddHours(9); // Her sabah 09:00

                var delay = targetTime - now;
                if (delay < TimeSpan.Zero)
                    delay = TimeSpan.FromDays(1); // zaman geçtiyse yarın sabahı bekle

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VendingContext>();

                    // Kritik stok kontrolü
                    var lowStockProducts = await context.Products
                        .Where(p => p.Stock < _stockThreshold)
                        .ToListAsync();

                    // Kritik bakiye kontrolü
                    var lowBalanceUsers = await context.AppUsers // AppUsers DbSet'i kullanılıyor
                        .Where(u => u.CurrentBalance < _balanceThreshold)
                        .ToListAsync();

                    if (!lowStockProducts.Any() && !lowBalanceUsers.Any())
                    {
                        _logger.LogInformation("Kritik stok veya bakiye problemi yok.");
                        continue;
                    }

                    var alertBody = new StringBuilder();
                    alertBody.AppendLine("🚨 Kritik Durum Bildirimi\n");

                    if (lowStockProducts.Any())
                    {
                        alertBody.AppendLine("📦 Düşük Stoklu Ürünler:");
                        foreach (var product in lowStockProducts)
                        {
                            alertBody.AppendLine($"• {product.Name} - {product.Stock} adet");
                        }
                        alertBody.AppendLine();
                    }

                    if (lowBalanceUsers.Any())
                    {
                        alertBody.AppendLine("💰 Düşük Bakiyeli Kullanıcılar:");
                        foreach (var user in lowBalanceUsers)
                        {
                            alertBody.AppendLine($"• Kullanıcı Kodu: {user.UserCode} - Bakiye: {user.CurrentBalance:C}");
                        }
                        alertBody.AppendLine();
                    }

                    await SendEmailAsync("Kritik Durum Raporu", alertBody.ToString());

                    _logger.LogInformation("Kritik durum bildirimi gönderildi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kritik bildirim gönderilirken hata oluştu.");
                }
            }
        }

        private async Task SendEmailAsync(string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("foodsvendingmachines@gmail.com", "ukniskyervovwrlc"), // uygulama şifresi!
                Timeout = 20000 // 20 saniye
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("foodsvendingmachines@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            // Yöneticilerin mail adreslerini ekleyin
            mailMessage.To.Add("ahmetseyyit94@gmail.com");
            mailMessage.To.Add("ahmetseyyit46@gmail.com");
            mailMessage.To.Add("ahmetseyyit54@gmail.com");

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
