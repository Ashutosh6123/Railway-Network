using MailService.Controllers;
using MailService.DTOs;
using MailService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MailService.Tests;

public class MailControllerTests
{
    [Test]
    public async Task Send_ReturnsOkWhenNotificationIsSent()
    {
        var service = new FakeMailNotificationService { Response = new SendMailResponse(true, "Notification sent successfully.") };
        var response = await CreateController(service).Send(CreateRequest(), "valid-internal-key");

        Assert.That(response.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(service.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Send_ReturnsUnauthorizedWhenInternalKeyIsInvalid()
    {
        var service = new FakeMailNotificationService();
        var response = await CreateController(service).Send(CreateRequest(), "invalid-key");

        Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(service.Requests, Is.Empty);
    }

    [Test]
    public async Task Send_ReturnsServiceUnavailableWhenSmtpDeliveryFails()
    {
        var service = new FakeMailNotificationService { Response = new SendMailResponse(false, "Notification could not be sent.") };
        var response = await CreateController(service).Send(CreateRequest(), "valid-internal-key");

        var result = response.Result as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
    }

    private static MailController CreateController(FakeMailNotificationService service)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["InternalService:ApiKey"] = "valid-internal-key" }).Build();
        return new MailController(service, configuration);
    }

    private static SendMailRequest CreateRequest() => new("passenger@example.com", "BookingWaitlisted", new Dictionary<string, string>
    {
        ["pnr"] = "PNR123", ["trainNumber"] = "12001", ["journeyDate"] = "2026-10-15", ["from"] = "Delhi", ["to"] = "Agra"
    });

    private sealed class FakeMailNotificationService : IMailNotificationService
    {
        public List<SendMailRequest> Requests { get; } = [];
        public SendMailResponse Response { get; init; } = new(true, "Notification sent successfully.");
        public Task<SendMailResponse> SendAsync(SendMailRequest request) { Requests.Add(request); return Task.FromResult(Response); }
    }
}
