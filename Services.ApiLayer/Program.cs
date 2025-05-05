#region Usings
using DinkToPdf.Contracts;
using DinkToPdf;
using Services.ApiLayer.Services_Balance_;
using Services.ApiLayer.Services_Invoice_.Abstract;
using Services.ApiLayer.Services_Report_;
using Services.ApiLayer.Vending.EntityLayer.Context;
#endregion
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MemoryCache
builder.Services.AddMemoryCache();

// Add IHttpClientFactory
builder.Services.AddHttpClient();

// Register VendingContext
builder.Services.AddDbContext<VendingContext>();

// Add other services
builder.Services.AddSingleton<RazorViewRenderService>();
builder.Services.AddSingleton<IConverter, SynchronizedConverter>(s => new SynchronizedConverter(new PdfTools()));
builder.Services.AddSingleton<PdfGeneratorService>();
builder.Services.AddScoped<EmailService>();

#region DinktoPDFAyarlari
var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "NativeLibrary", "libwkhtmltox.dll"));
#endregion

#region Servisler(Raporlama-Mail)
builder.Services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();
builder.Services.AddHostedService<InvoiceTimerService>();
builder.Services.AddScoped<BalanceService>();
builder.Services.AddHostedService<MonthlyBalanceResetService>();
builder.Services.AddHostedService<CriticalAlertsService>();
builder.Services.AddHostedService<DailySpendingReportBackgroundService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddHostedService<ReportBackgroundService>();
builder.Services.AddHostedService<ReportTimerService>();
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
