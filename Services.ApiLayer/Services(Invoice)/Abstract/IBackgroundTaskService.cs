namespace Services.ApiLayer.Services_Invoice_.Abstract
{
    public interface IBackgroundTaskService
    {
        Task GenerateInvoiceAndSendEmailAsync(InvoiceViewModel invoiceModel);
    }
}
