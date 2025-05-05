using Services.ApiLayer.Vending.EntityLayer.Context;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Services.ApiLayer.Services_Balance_
{
    public class DailySpendingReportBackgroundService : BackgroundService
    {
        private readonly ILogger<DailySpendingReportBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public DailySpendingReportBackgroundService(ILogger<DailySpendingReportBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var targetTime = DateTime.Today.AddDays(1).AddMinutes(-1); // 23:59

                var delay = targetTime - now;
                if (delay < TimeSpan.Zero)
                    delay = TimeSpan.FromDays(1); // Geç kaldıysa ertesi günü bekle

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VendingContext>();

                    var today = DateTime.UtcNow.Date;

                    var orders = await context.Orders
                        .Where(o => o.OrderDate.Date == today)
                        .Include(o => o.Product)
                        .ToListAsync();

                    if (!orders.Any())
                    {
                        _logger.LogInformation("Bugünkü sipariş bulunamadı. Rapor gönderilmedi.");
                        continue;
                    }

                    var reportBuilder = new StringBuilder();
                    reportBuilder.AppendLine($"📊 {today:yyyy-MM-dd} Günlük Harcama Raporu\n");

                    var grouped = orders
                        .GroupBy(o => o.UserCode)
                        .Select(g => new
                        {
                            UserCode = g.Key,
                            TotalSpent = g.Sum(x => x.TotalPrice),
                            ProductDetails = g.Select(x => $"{x.Product.Name} x{x.Quantity} = {x.TotalPrice:C}").ToList()
                        });

                    foreach (var user in grouped)
                    {
                        reportBuilder.AppendLine($"👤 Kullanıcı: {user.UserCode}");
                        foreach (var detail in user.ProductDetails)
                            reportBuilder.AppendLine($"   • {detail}");
                        reportBuilder.AppendLine($"   ➤ Toplam: {user.TotalSpent:C}\n");
                    }

                    await SendEmailAsync("Günlük Harcama Raporu", reportBuilder.ToString());

                    _logger.LogInformation("Günlük harcama raporu gönderildi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Günlük rapor gönderilirken hata oluştu.");
                }
            }
        }

        private async Task SendEmailAsync(string subject, string body)
        {
            // SMTP bilgilerini buraya yaz
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("foodsvendingmachines@gmail.com", "uygun-uygun-uygun"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("foodsvendingmachines@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            // Buraya yöneticilerin mail adreslerini ekle
            mailMessage.To.Add("ahmetseyyit94@gmail.com");
            mailMessage.To.Add("ahmetseyyit46@gmail.com");
            mailMessage.To.Add("ahmetseyyit54@gmail.com");

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
