using Microsoft.EntityFrameworkCore;
using Services.ApiLayer.Vending.EntityLayer.Context;

namespace Services.ApiLayer.Services_Balance_
{
    public class BalanceService
    {
        private readonly VendingContext _context;
        private readonly ILogger<BalanceService> _logger;

        public BalanceService(VendingContext context, ILogger<BalanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Kullanıcının bakiyesinin yeterli olup olmadığını kontrol eder
        public async Task<bool> HasSufficientBalanceAsync(long userCode, decimal totalPrice)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCode == userCode);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", userCode);
                return false;
            }

            if (user.CurrentBalance < totalPrice)
            {
                _logger.LogWarning("Yetersiz bakiye: Kullanıcı {UserCode}, Bakiyesi: {CurrentBalance}, İstediği: {TotalPrice}",
                    userCode, user.CurrentBalance, totalPrice);
                return false;
            }

            return true;
        }

        // Kullanıcının bakiyesinden belirtilen tutarı düşer
        public async Task DeductBalanceAsync(long userCode, decimal totalPrice)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCode == userCode);
            if (user != null)
            {
                user.CurrentBalance -= totalPrice;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Kullanıcı bakiyesi güncellendi: {UserCode}, Yeni Bakiye: {CurrentBalance}",
                    userCode, user.CurrentBalance);
            }
            else
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {UserCode}", userCode);
            }
        }
    }
}
