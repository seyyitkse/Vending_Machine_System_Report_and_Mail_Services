using Microsoft.EntityFrameworkCore;
using Services.ApiLayer.Vending.EntityLayer.Context;

namespace Services.ApiLayer.Services_Balance_
{
    public class MonthlyBalanceResetService : BackgroundService
    {
        private readonly ILogger<MonthlyBalanceResetService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime _lastRunDate = DateTime.MinValue;

        public MonthlyBalanceResetService(ILogger<MonthlyBalanceResetService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (DateTime.UtcNow.Day == 1 && _lastRunDate.Date != DateTime.UtcNow.Date)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<VendingContext>();
                        var users = await context.Users.ToListAsync();

                        foreach (var user in users)
                        {
                            user.CurrentBalance = user.MonthlyLimit;
                        }

                        await context.SaveChangesAsync();
                        _logger.LogInformation("Aylık bakiye sıfırlama işlemi tamamlandı: {Time}", DateTime.UtcNow);
                    }

                    _lastRunDate = DateTime.UtcNow.Date;
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Her saat kontrol et
            }
        }
    }
}
