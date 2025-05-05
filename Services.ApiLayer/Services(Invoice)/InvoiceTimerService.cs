public class InvoiceTimerService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly ILogger<InvoiceTimerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public InvoiceTimerService(ILogger<InvoiceTimerService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InvoiceTimerService başlatıldı.");

        // 10 dakikada bir çalışacak şekilde ayarlıyoruz (ilk 2 dakika sonra başlasın)
        _timer = new Timer(SendInvoiceRequest, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    private async void SendInvoiceRequest(object state)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("https://localhost:44304/api/Order/send-oldest-unpaid-invoice", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Fatura gönderim endpoint’i başarıyla çağrıldı.");
            }
            else
            {
                _logger.LogWarning("Fatura endpoint çağrısı başarısız: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatura endpoint’ine istek atılırken hata oluştu.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InvoiceTimerService durduruluyor...");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
