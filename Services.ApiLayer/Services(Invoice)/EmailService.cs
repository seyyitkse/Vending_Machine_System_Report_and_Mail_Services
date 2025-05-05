using System.Net.Mail;
using System.Net;

public class EmailService
{
    public async Task SendInvoiceEmailAsync(string toEmail, string toName, byte[] pdfBytes)
    {
        var fromAddress = new MailAddress("foodsvendingmachines@gmail.com", "Food Vending Machines");
        var toAddress = new MailAddress(toEmail, toName);
        const string subject = "Satın Aldığınız Ürünün Faturası";
        const string body = "Merhaba, ürün satın alma işleminizin faturası ekte PDF olarak sunulmuştur. Teşekkür ederiz.";

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("foodsvendingmachines@gmail.com", "ukniskyervovwrlc"), // uygulama şifresi!
            Timeout = 20000 // 20 saniye
        };

        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            // PDF ekini oluştur
            message.Attachments.Add(new Attachment(
                new MemoryStream(pdfBytes),
                "Fatura.pdf",
                "application/pdf"
            ));

            await smtp.SendMailAsync(message);
        }
    }


    private readonly string _smtpServer = "smtp.gmail.com"; // SMTP sunucusu
    private readonly int _smtpPort = 587; // SMTP portu
    private readonly string _smtpUser = "foodsvendingmachines@gmail.com"; // SMTP kullanıcı adı
    private readonly string _smtpPassword = "ukniskyervovwrlc"; // SMTP şifresi

    public async Task SendEmailAsync(string toEmail, string subject, string body, byte[] attachmentData = null, string attachmentName = null)
    {

        var currentDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        body += $"\n\nİşlem Tarihi: {currentDate}"; using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpUser),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        mailMessage.To.Add(toEmail);

        // Ek varsa ekle
        if (attachmentData?.Length > 0 && !string.IsNullOrEmpty(attachmentName))
        {
            mailMessage.Attachments.Add(new Attachment(new MemoryStream(attachmentData), attachmentName));
        }

        await client.SendMailAsync(mailMessage);
    }

}