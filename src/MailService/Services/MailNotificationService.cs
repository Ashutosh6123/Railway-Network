using MailService.DTOs;
using MailService.Email;

namespace MailService.Services;

public sealed class MailNotificationService(
    ISmtpMailSender smtpMailSender,
    ILogger<MailNotificationService> logger) : IMailNotificationService
{
    public async Task<SendMailResponse> SendAsync(SendMailRequest request)
    {
        ValidateRequest(request);
        var emailMessage = BuildEmailMessage(request);

        try
        {
            await smtpMailSender.SendAsync(emailMessage);
            return new SendMailResponse(true, "Notification sent successfully.");
        }
        catch (Exception exception)
        {
            request.Data.TryGetValue("pnr", out var pnr);
            logger.LogError(exception, "SMTP delivery failed for template {Template} and PNR {Pnr}.", request.Template, pnr ?? "not supplied");
            return new SendMailResponse(false, "Notification could not be sent.");
        }
    }

    private static void ValidateRequest(SendMailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.To)) throw new ArgumentException("Recipient email address is required.");
        if (string.IsNullOrWhiteSpace(request.Template)) throw new ArgumentException("Notification template is required.");
        if (request.Data is null) throw new ArgumentException("Notification data is required.");
    }

    private static EmailMessage BuildEmailMessage(SendMailRequest request)
    {
        return request.Template switch
        {
            "BookingConfirmed" => new EmailMessage(request.To, "Railway booking confirmed",
                $"Your booking with PNR {GetValue(request.Data, "pnr")} is confirmed. Train: {GetValue(request.Data, "trainNumber")}. Journey: {GetValue(request.Data, "journeyDate")} from {GetValue(request.Data, "from")} to {GetValue(request.Data, "to")}.{Environment.NewLine}{GetValue(request.Data, "passengerSeats")}"),
            "BookingWaitlisted" => new EmailMessage(request.To, "Railway booking waitlisted",
                $"Your booking with PNR {GetValue(request.Data, "pnr")} is waitlisted. Train: {GetValue(request.Data, "trainNumber")}. Journey: {GetValue(request.Data, "journeyDate")} from {GetValue(request.Data, "from")} to {GetValue(request.Data, "to")}. Current waitlist position: {GetValue(request.Data, "waitlistPosition")}."),
            "Cancellation" => new EmailMessage(request.To, "Railway booking cancelled",
                $"Your booking with PNR {GetValue(request.Data, "pnr")} has been cancelled. Train: {GetValue(request.Data, "trainNumber")}. Journey: {GetValue(request.Data, "journeyDate")} from {GetValue(request.Data, "from")} to {GetValue(request.Data, "to")}."),
            "WaitlistPromotion" => new EmailMessage(request.To, "Railway booking confirmed from waitlist",
                $"Your booking with PNR {GetValue(request.Data, "pnr")} has been confirmed from the waitlist. Train: {GetValue(request.Data, "trainNumber")}. Journey: {GetValue(request.Data, "journeyDate")} from {GetValue(request.Data, "from")} to {GetValue(request.Data, "to")}.{Environment.NewLine}{GetValue(request.Data, "passengerSeats")}"),
            _ => throw new ArgumentException("Unsupported notification template.")
        };
    }

    private static string GetValue(Dictionary<string, string> data, string key)
    {
        if (!data.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Notification data field '{key}' is required.");
        }

        return value;
    }
}
