namespace ReservationService.Clients;

public interface IUserClient
{
    Task<UserClientDto?> GetUserAsync(int userId);
}
