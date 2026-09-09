using System.Net;
using System.Net.Mail;
using MailService.DTOs;
using Microsoft.Extensions.Options;

namespace MailService.Email;

public sealed class SmtpMailSender(IOptions<SmtpSettings> smtpOptions) : ISmtpMailSender
{
    public async Task SendAsync(EmailMessage message)
    {
        var settings = smtpOptions.Value;

        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromAddress))
        {
            throw new InvalidOperationException("SMTP host and from address must be configured.");
        }

        using var mailMessage = new MailMessage(settings.FromAddress, message.To, message.Subject, message.Body);
        using var smtpClient = new SmtpClient(settings.Host, settings.Port) { EnableSsl = settings.EnableSsl };

        if (!string.IsNullOrWhiteSpace(settings.Username))
        {
            smtpClient.Credentials = new NetworkCredential(settings.Username, settings.Password);
        }

        await smtpClient.SendMailAsync(mailMessage);
    }
}
