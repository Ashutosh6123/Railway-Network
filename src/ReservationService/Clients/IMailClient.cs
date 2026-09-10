namespace ReservationService.Clients;

public interface IMailClient
{
    Task<MailClientResult> SendAsync(string to, string template, Dictionary<string, string> data);
}
