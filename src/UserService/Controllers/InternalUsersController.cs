using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/internal/users")]
public class InternalUsersController(IUserService userService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("{userId:int}")]
    public async Task<ActionResult<UserDto>> GetUser(
        int userId,
        [FromHeader(Name = "X-Internal-Service-Key")] string? internalServiceKey)
    {
        if (!HasValidInternalServiceKey(internalServiceKey))
        {
            return Unauthorized();
        }

        var user = await userService.GetByIdAsync(userId);

        return user is null ? NotFound() : Ok(user);
    }

    private bool HasValidInternalServiceKey(string? suppliedKey)
    {
        var configuredKey = configuration["InternalService:ApiKey"];

        return !string.IsNullOrWhiteSpace(suppliedKey) &&
               !string.IsNullOrWhiteSpace(configuredKey) &&
               string.Equals(suppliedKey, configuredKey, StringComparison.Ordinal);
    }
}
