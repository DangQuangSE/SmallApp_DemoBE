namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Builds email subject and HTML body for transactional emails.
/// Keeps presentation logic out of domain/application services.
/// </summary>
public interface IEmailTemplateService
{
    (string Subject, string Body) BuildOtpEmail(string username, string otpCode);
}
