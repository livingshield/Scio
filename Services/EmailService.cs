using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace ScioApp.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent, string? attachmentFileName = null, byte[]? attachmentData = null);
    Task SendBulkInviteAsync(string[] toEmails, string groupName, string inviteCode, string inviteUrl);
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

    public async Task SendBulkInviteAsync(string[] toEmails, string groupName, string inviteCode, string inviteUrl)
    {
        var emailsString = string.Join(";", toEmails);
        var subject = $"Pozvánka do skupiny: {groupName}";
        var body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                <h2 style='color: #4f46e5;'>Vítej v aplikaci Scio</h2>
                <p>Byl jsi pozván do skupiny: <strong>{groupName}</strong></p>
                <p>Kód pro připojení: <span style='font-size: 1.25rem; font-weight: bold; color: #4f46e5;'>{inviteCode}</span></p>
                <div style='margin-top: 30px; text-align: center;'>
                    <a href='{inviteUrl}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>Připojit se ke skupině</a>
                </div>
            </div>";
        
        await SendEmailAsync(emailsString, subject, body);
    }
}
