using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ReportTimerService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly ILogger<ReportTimerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ReportTimerService(ILogger<ReportTimerService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ReportTimerService başlatıldı.");

        // İlk çalıştırma zamanı ve tekrar aralığı
        var now = DateTime.Now;
        var targetTime = new TimeSpan(14, 25, 0); // Her gün saat 14:25'te çalıştır
        var firstRun = now.Date.Add(targetTime) > now ? now.Date.Add(targetTime) : now.Date.AddDays(1).Add(targetTime);
        var initialDelay = firstRun - now;

        _timer = new Timer(ExecuteReports, null, initialDelay, TimeSpan.FromDays(1)); // Her gün çalıştır
        return Task.CompletedTask;
    }

    private async void ExecuteReports(object state)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // Günlük rapor gönderimi
            _logger.LogInformation("Günlük rapor gönderimi başlatıldı.");
            var dailyResponse = await client.PostAsync("https://localhost:44304/api/Report/send-daily-report", null);
            if (dailyResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Günlük rapor başarıyla gönderildi.");
            }
            else
            {
                _logger.LogWarning("Günlük rapor gönderimi başarısız: {StatusCode}", dailyResponse.StatusCode);
            }

            // Aylık rapor gönderimi (Ayın ilk günü)
            if (DateTime.Now.Day == 1)
            {
                _logger.LogInformation("Aylık rapor gönderimi başlatıldı.");
                var monthlyResponse = await client.PostAsync("https://localhost:44304/api/Report/send-monthly-report", null);
                if (monthlyResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Aylık rapor başarıyla gönderildi.");
                }
                else
                {
                    _logger.LogWarning("Aylık rapor gönderimi başarısız: {StatusCode}", monthlyResponse.StatusCode);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor gönderimi sırasında bir hata oluştu.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ReportTimerService durduruluyor...");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}