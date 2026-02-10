using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace ScioApp.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent, string? attachmentFileName = null, byte[]? attachmentData = null);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent, string? attachmentFileName = null, byte[]? attachmentData = null)
    {
        var email = new MimeMessage();
        var fromName = _config["Smtp:FromName"] ?? "Scio App";
        var fromEmail = _config["Smtp:FromEmail"] ?? "scio@ekobio.org";
        
        email.From.Add(new MailboxAddress(fromName, fromEmail));
        
        // Handle multiple recipients separated by semicolon
        var recipients = to.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var recipient in recipients)
        {
            email.To.Add(MailboxAddress.Parse(recipient));
        }

        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlContent };

        if (attachmentData != null && !string.IsNullOrEmpty(attachmentFileName))
        {
            builder.Attachments.Add(attachmentFileName, attachmentData);
        }

        email.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var username = _config["Smtp:Username"];
            var password = _config["Smtp:Password"];

            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(email);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            throw;
        }
    }
}
