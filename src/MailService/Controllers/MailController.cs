using MailService.DTOs;
using MailService.Services;
using Microsoft.AspNetCore.Mvc;

namespace MailService.Controllers;

[ApiController]
[Route("api/internal/mail")]
public sealed class MailController(IMailNotificationService mailNotificationService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("send")]
    public async Task<ActionResult<SendMailResponse>> Send(
        SendMailRequest request,
        [FromHeader(Name = "X-Internal-Service-Key")] string? internalServiceKey)
    {
        if (!HasValidInternalServiceKey(internalServiceKey)) return Unauthorized();

        var response = await mailNotificationService.SendAsync(request);
        return response.Success ? Ok(response) : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    private bool HasValidInternalServiceKey(string? suppliedKey)
    {
        var configuredKey = configuration["InternalService:ApiKey"];
        return !string.IsNullOrWhiteSpace(suppliedKey) && !string.IsNullOrWhiteSpace(configuredKey) &&
               string.Equals(suppliedKey, configuredKey, StringComparison.Ordinal);
    }
}
