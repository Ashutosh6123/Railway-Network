using MailService.DTOs;

namespace MailService.Email;

public interface ISmtpMailSender
{
    Task SendAsync(EmailMessage message);
}
