using Services.ApiLayer.Services_Report_;

public class ReportBackgroundService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReportBackgroundService> _logger;

    public ReportBackgroundService(IServiceProvider serviceProvider, ILogger<ReportBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ReportBackgroundService başlatıldı.");

        // İlk çalıştırma zamanı ve tekrar aralığı
        var now = DateTime.Now;
        var targetTime = new TimeSpan(10, 50, 0); // Her gün saat 10:30'da çalıştır
        var firstRun = now.Date.Add(targetTime) > now ? now.Date.Add(targetTime) : now.Date.AddDays(1).Add(targetTime);
        var initialDelay = firstRun - now;


        _timer = new Timer(ExecuteReports, null, initialDelay, TimeSpan.FromDays(1)); // Her gün çalıştır
        return Task.CompletedTask;
    }

    private async void ExecuteReports(object state)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();

            try
            {
                _logger.LogInformation("Günlük rapor gönderimi başlatıldı.");
                await reportService.SendReportToAdminsAsync("daily");

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    _logger.LogInformation("Haftalık rapor gönderimi başlatıldı.");
                    await reportService.SendReportToAdminsAsync("weekly");
                }

                if (DateTime.Now.Day == 1)
                {
                    _logger.LogInformation("Aylık rapor gönderimi başlatıldı.");
                    await reportService.SendReportToAdminsAsync("monthly");
                }

                _logger.LogInformation("Rapor gönderim işlemleri tamamlandı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rapor gönderimi sırasında bir hata oluştu.");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ReportBackgroundService durduruluyor...");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
