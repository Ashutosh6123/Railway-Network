namespace MailService.DTOs;

public record SendMailRequest(
    string To,
    string Template,
    Dictionary<string, string> Data);

public record SendMailResponse(bool Success, string Message);

public record EmailMessage(string To, string Subject, string Body);
