using DinkToPdf;
using DinkToPdf.Contracts;
using RazorLight;

public class InvoiceService
{
    private readonly RazorLightEngine _razorEngine;
    private static readonly IConverter _pdfConverter = new SynchronizedConverter(new PdfTools());
    private static readonly object _pdfLock = new object();

    public InvoiceService()
    {
        _razorEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(InvoiceViewModel model)
    {
        try
        {
            string htmlContent = await _razorEngine.CompileRenderAsync("InvoiceTemplate.cshtml", model);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects = {
                    new ObjectSettings
                    {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            lock (_pdfLock)
            {
                return _pdfConverter.Convert(doc);
            }
        }
        catch (Exception ex)
        {
            // Logging yapılabilir
            throw;
        }
    }
}