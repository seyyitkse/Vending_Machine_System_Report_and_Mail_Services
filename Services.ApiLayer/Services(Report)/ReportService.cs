using DinkToPdf.Contracts;
using DinkToPdf;
using Services.ApiLayer.Vending.EntityLayer.Context;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Services.ApiLayer.Vending.EntityLayer.Concrete;
namespace Services.ApiLayer.Services_Report_
{
    public class ReportService
    {
        private readonly IConverter _converter;
        private readonly VendingContext _context;
        private readonly EmailService _emailService;

        public ReportService(IConverter converter, VendingContext context, EmailService emailService)
        {
            _converter = converter;
            _context = context;
            _emailService = emailService;
        }

        public async Task<byte[]> GenerateDailyReportAsync()
        {
            var today = DateTime.Today;

            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .Where(o => o.OrderDate.Date == today)
                .ToListAsync();

            return GenerateReport("Günlük Satış Raporu", orders);
        }

        public async Task<byte[]> GenerateWeeklyReportAsync()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .Where(o => o.OrderDate.Date >= startOfWeek && o.OrderDate.Date <= today)
                .ToListAsync();

            return GenerateReport($"Haftalık Satış Raporu ({startOfWeek:dd.MM.yyyy} - {today:dd.MM.yyyy})", orders);
        }

        public async Task<byte[]> GenerateMonthlyReportAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .Where(o => o.OrderDate.Date >= startOfMonth && o.OrderDate.Date <= today)
                .ToListAsync();

            return GenerateReport($"Aylık Satış Raporu ({startOfMonth:dd.MM.yyyy} - {today:dd.MM.yyyy})", orders);
        }

        public async Task<byte[]> GenerateCustomReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .Include(o => o.Product)
                .Where(o => o.OrderDate.Date >= startDate && o.OrderDate.Date <= endDate)
                .ToListAsync();

            return GenerateReport($"Özel Satış Raporu ({startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy})", orders);
        }
        public async Task<byte[]> GenerateYearlyReportAsync()
        {
            var currentYear = DateTime.Today.Year;
            var startOfYear = new DateTime(currentYear, 1, 1);
            var endOfYear = new DateTime(currentYear, 12, 31);

            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .Where(o => o.OrderDate.Date >= startOfYear && o.OrderDate.Date <= endOfYear)
                .ToListAsync();

            return GenerateReport($"Yıllık Satış Raporu ({startOfYear:dd.MM.yyyy} - {endOfYear:dd.MM.yyyy})", orders);
        }

        private byte[] GenerateReport(string title, List<Order> orders)
        {
            var html = new StringBuilder();

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalPrice);

            // CSS ve HTML içerik
            html.Append(@"
<style>
    body {
        font-family: Arial, sans-serif;
        margin: 20px;
        color: #333;
        font-size: 12px;
    }
    h1, h2 {
        text-align: center;
        color: #4CAF50;
        page-break-after: avoid;
    }
    table {
        width: 100%;
        border-collapse: collapse;
        margin: 30px 0;
        page-break-inside: auto;
    }
    thead {
        display: table-header-group;
    }
    tr {
        page-break-inside: avoid;
        page-break-after: auto;
    }
    th, td {
        border: 1px solid #ddd;
        padding: 8px;
        text-align: left;
        vertical-align: top;
    }
    th {
        background-color: #f4f4f4;
        color: #333;
    }
    tr:nth-child(even) {
        background-color: #f9f9f9;
    }
    tr:hover {
        background-color: #f1f1f1;
    }
</style>

");

            // Başlık
            html.Append($"<h1>{title}</h1>");

            // Özet Tablo
            html.Append("<h2>Özet Tablo</h2>");
            html.Append("<table class='summary-table'>");
            html.Append("<thead>");
            html.Append("<tr><th>Toplam Sipariş Sayısı</th><th>Toplam Gelir</th></tr>");
            html.Append("</thead>");
            html.Append("<tbody>");
            html.Append($"<tr><td>{totalOrders}</td><td>{totalRevenue:C}</td></tr>");
            html.Append("</tbody>");
            html.Append("</table>");

            // Sipariş Detay Tablosu
            //html.Append("<table>");
            //html.Append("<thead>");
            //html.Append("<tr><th>Sipariş Numarası</th><th>Kullanıcı Adı</th><th>Toplam Tutar</th><th>Tarih</th></tr>");
            //html.Append("</thead>");
            //html.Append("<tbody>");
            html.Append(@"
        <table>
            <thead>
                <tr>
                    <th>Sipariş Numarası</th>
                    <th>Kullanıcı Adı</th>
                    <th>Toplam Tutar</th>
                    <th>Tarih</th>
                </tr>
            </thead>
        <tbody>");

            foreach (var order in orders)
            {
                html.Append($"<tr><td>{order.OrderId}</td><td>{order.AppUser.UserName}</td><td>{order.TotalPrice:C}</td><td>{order.OrderDate:dd.MM.yyyy HH:mm}</td></tr>");
            }

            html.Append("</tbody>");
            html.Append("</table>");

            // Bilgi notu (sayfanın sonuna)
            html.Append("<p style='margin-top: 50px; font-style: italic; text-align: center; color: gray;'>");
            html.Append("Bu belge resmî değildir, yalnızca bilgi amaçlıdır.");
            html.Append("</p>");

            html.Append(@"
    <div style='margin-top: 30px; text-align: center; font-family: Georgia, serif; color: #555;'>
        <hr style='margin-bottom: 10px; width: 60%; border: none; border-top: 1px solid #ccc;'/>
        <div style='font-size: 16px; font-weight: bold;'>RAPIDVEND<sup style='font-size:10px;'>®</sup> Akıllı Otomat Sistemleri</div>
        <div style='font-size: 12px; margin-top: 5px;'>© 2025 - Tüm Hakları Saklıdır.</div>
    </div>
");


            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 }
            },
                Objects = {
                new ObjectSettings {
                    PagesCount = true,
                    HtmlContent = html.ToString(),
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
            };

            return _converter.Convert(doc);

        }

        public async Task SendReportToAdminsAsync(string reportType)
        {
            byte[] reportData;
            string reportName;

            // Rapor türüne göre rapor oluştur
            switch (reportType.ToLower())
            {
                case "daily":
                    reportData = await GenerateDailyReportAsync();
                    reportName = "Günlük Satış Raporu";
                    break;
                case "weekly":
                    reportData = await GenerateWeeklyReportAsync();
                    reportName = "Haftalık Satış Raporu";
                    break;
                case "monthly":
                    reportData = await GenerateMonthlyReportAsync();
                    reportName = "Aylık Satış Raporu";
                    break;
                case "yearly":
                    reportData = await GenerateYearlyReportAsync();
                    reportName = "Yıllık Satış Raporu";
                    break;
                default:
                    throw new ArgumentException("Geçersiz rapor türü.");
            }

            // Admin kullanıcıları al
            var adminUsers = await _context.AppUsers
                .Where(user => user.IsAdmin)
                .ToListAsync();

            if (!adminUsers.Any())
                throw new InvalidOperationException("Sistemde admin kullanıcı bulunamadı.");

            // Email gönderim servisini kullanarak raporu gönder
            foreach (var admin in adminUsers)
            {
                var emailBody = $"Merhaba {admin.FullName},\n\n{reportName} raporu ektedir.";
                await _emailService.SendEmailAsync(admin.Email, $"{reportName}", emailBody, reportData, $"{reportName}.pdf");
            }
        }

    }
}
