namespace BusinessKit.Application.Email;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
}
