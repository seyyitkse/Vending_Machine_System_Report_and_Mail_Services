using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.ApiLayer.Services_Balance_;
using Services.ApiLayer.Services_Invoice_.Abstract;
using Services.ApiLayer.Vending.EntityLayer.Context;

namespace Services.ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly VendingContext _context;
        private readonly IBackgroundTaskService _backgroundTaskService;
        private readonly BalanceService _balanceService;

        public OrderController(VendingContext context, IBackgroundTaskService backgroundTaskService, BalanceService balanceService)
        {
            _context = context;
            _backgroundTaskService = backgroundTaskService;
            _balanceService = balanceService;
        }

        [HttpPost("send-oldest-unpaid-invoice")]
        public async Task<IActionResult> SendOldestUnpaidInvoice()
        {
            try
            {
                // En eski gönderilmemiş siparişi getir
                var pendingOrder = await _context.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Vend)
                    .Include(o => o.AppUser)
                    .Where(o => !o.IsInvoiceSent)
                    .OrderBy(o => o.OrderDate)
                    .FirstOrDefaultAsync();

                if (pendingOrder == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Gönderilmemiş fatura bulunamadı."
                    });
                }

                // Fatura modeli oluştur
                var invoiceModel = new InvoiceViewModel
                {
                    OrderId = pendingOrder.OrderId,
                    UserName = pendingOrder.AppUser.UserName,
                    Email = pendingOrder.AppUser.Email,
                    ProductName = pendingOrder.Product.Name,
                    Quantity = pendingOrder.Quantity,
                    Price = pendingOrder.Product.Price,
                    TotalPrice = pendingOrder.TotalPrice,
                    OrderDate = pendingOrder.OrderDate,
                    VendName = pendingOrder.Vend?.Name ?? "Bilinmiyor"
                };

                // Fatura oluştur ve e-posta ile gönder
                await _backgroundTaskService.GenerateInvoiceAndSendEmailAsync(invoiceModel);

                // Siparişi işaretle
                pendingOrder.IsInvoiceSent = true;
                _context.Orders.Update(pendingOrder);
                await _context.SaveChangesAsync();


                // Başarılı yanıt dön
                return Ok(new
                {
                    success = true,
                    message = "Fatura başarıyla gönderildi.",
                    invoice = invoiceModel
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Fatura gönderimi sırasında bir hata oluştu.",
                    error = ex.Message
                });
            }
        }
    }
}
