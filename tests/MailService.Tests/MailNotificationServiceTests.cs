using MailService.DTOs;
using MailService.Email;
using MailService.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace MailService.Tests;

public class MailNotificationServiceTests
{
    [TestCase("BookingConfirmed")]
    [TestCase("BookingWaitlisted")]
    [TestCase("Cancellation")]
    [TestCase("WaitlistPromotion")]
    public async Task SendAsync_BuildsAndSendsSupportedTemplate(string template)
    {
        var sender = new FakeSmtpMailSender();
        var response = await CreateService(sender).SendAsync(CreateRequest(template));

        Assert.That(response.Success, Is.True);
        Assert.That(sender.Messages, Has.Count.EqualTo(1));
        Assert.That(sender.Messages[0].Subject, Is.Not.Empty);
        Assert.That(sender.Messages[0].Body, Does.Contain("PNR123"));
    }

    [Test]
    public void SendAsync_RejectsUnsupportedTemplate()
    {
        Assert.ThrowsAsync<ArgumentException>(() => CreateService(new FakeSmtpMailSender()).SendAsync(CreateRequest("Unknown")));
    }

    [Test]
    public void SendAsync_RejectsMissingRequiredTemplateData()
    {
        var request = new SendMailRequest("passenger@example.com", "BookingConfirmed", new Dictionary<string, string> { ["pnr"] = "PNR123" });
        Assert.ThrowsAsync<ArgumentException>(() => CreateService(new FakeSmtpMailSender()).SendAsync(request));
    }

    [Test]
    public async Task SendAsync_ReturnsFailureWhenSmtpSenderThrows()
    {
        var sender = new FakeSmtpMailSender { ExceptionToThrow = new InvalidOperationException("SMTP unavailable") };
        var response = await CreateService(sender).SendAsync(CreateRequest("BookingConfirmed"));

        Assert.That(response.Success, Is.False);
        Assert.That(response.Message, Is.EqualTo("Notification could not be sent."));
    }

    private static MailNotificationService CreateService(FakeSmtpMailSender sender) => new(sender, NullLogger<MailNotificationService>.Instance);

    private static SendMailRequest CreateRequest(string template) => new(
        "passenger@example.com", template,
        new Dictionary<string, string>
        {
            ["pnr"] = "PNR123", ["trainNumber"] = "12001", ["journeyDate"] = "2026-10-15",
            ["from"] = "Delhi", ["to"] = "Agra", ["coach"] = "S1", ["seat"] = "25"
        });

    private sealed class FakeSmtpMailSender : ISmtpMailSender
    {
        public List<EmailMessage> Messages { get; } = [];
        public Exception? ExceptionToThrow { get; init; }
        public Task SendAsync(EmailMessage message)
        {
            if (ExceptionToThrow is not null) throw ExceptionToThrow;
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
