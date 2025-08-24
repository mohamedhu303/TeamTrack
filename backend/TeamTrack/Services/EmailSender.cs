using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromEmail = _config["EmailSettings:FromEmail"];
        var appPassword = _config["EmailSettings:AppPassword"];

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail, appPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
        await smtpClient.SendMailAsync(mailMessage);
    }
}
