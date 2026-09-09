namespace UserService.DTOs;
public record RegisterRequest(string Name,string Email,string PhoneNumber,string Password);
public record LoginRequest(string Email,string Password);
public record LoginResponse(string Token,int UserId,string Role,int ExpiresIn);
public record UserDto(int Id,string Name,string Email,string PhoneNumber,string Role);
