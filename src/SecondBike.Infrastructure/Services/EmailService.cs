using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// SMTP email service. Reads configuration from appsettings "Email" section.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("Email");
        var smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var senderEmail = emailSettings["SenderEmail"];
        var senderPassword = emailSettings["SenderPassword"];
        var senderName = emailSettings["SenderName"] ?? "SecondBike";

        if (string.IsNullOrEmpty(senderEmail) ||
            senderEmail == "your-email@gmail.com" ||
            string.IsNullOrEmpty(senderPassword) ||
            senderPassword == "your-app-password")
        {
            _logger.LogWarning(
                "[DEV MODE] Email not configured. Would send to {To} | Subject: {Subject} | Body: {Body}",
                to, subject, body);
            return;
        }

        using var message = new MailMessage();
        message.From = new MailAddress(senderEmail, senderName);
        message.To.Add(new MailAddress(to));
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(smtpHost, smtpPort);
        client.Credentials = new NetworkCredential(senderEmail, senderPassword);
        client.EnableSsl = true;

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
    }
}
