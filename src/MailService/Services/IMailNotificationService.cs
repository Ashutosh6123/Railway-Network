using MailService.DTOs;

namespace MailService.Services;

public interface IMailNotificationService
{
    Task<SendMailResponse> SendAsync(SendMailRequest request);
}
