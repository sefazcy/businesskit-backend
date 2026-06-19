using BusinessKit.Application.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BusinessKit.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("Email disabled (EmailSettings:Enabled=false). Skipping send to {ToEmail}.", toEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Email send skipped: recipient address is blank.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Host) ||
            string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogWarning(
                "Email send skipped: required SMTP configuration (Host, Username, Password, FromEmail) is incomplete.");
            return;
        }

        try
        {
            var message = new MimeMessage();

            var from = string.IsNullOrWhiteSpace(_settings.FromName)
                ? new MailboxAddress(_settings.FromEmail, _settings.FromEmail)
                : new MailboxAddress(_settings.FromName, _settings.FromEmail);

            var to = string.IsNullOrWhiteSpace(toName)
                ? new MailboxAddress(toEmail, toEmail)
                : new MailboxAddress(toName, toEmail);

            message.From.Add(from);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();

            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {ToEmail} — subject: {Subject}.", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} — subject: {Subject}.", toEmail, subject);
        }
    }
}
