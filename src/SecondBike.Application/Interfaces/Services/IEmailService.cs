namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
