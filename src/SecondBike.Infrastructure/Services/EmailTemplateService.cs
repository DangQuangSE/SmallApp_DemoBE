using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Builds HTML email content for transactional emails.
/// Single place to maintain all email templates.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    public (string Subject, string Body) BuildOtpEmail(string username, string otpCode)
    {
        const string subject = "Your verification code — SecondBike";

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #2563eb;">Email Verification — SecondBike</h2>
                <p>Hi <strong>{username}</strong>,</p>
                <p>Thank you for signing up for SecondBike. Please use the verification code below to confirm your email address:</p>
                <div style="text-align: center; margin: 30px 0;">
                    <div style="display: inline-block; background-color: #f3f4f6; border: 2px dashed #2563eb;
                                padding: 16px 40px; border-radius: 8px; letter-spacing: 8px;
                                font-size: 32px; font-weight: bold; color: #1e40af;">
                        {otpCode}
                    </div>
                </div>
                <p style="color: #666; font-size: 14px;">This code will expire in <strong>10 minutes</strong>.</p>
                <p style="color: #666; font-size: 14px;">If you did not create an account, please ignore this email.</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="color: #999; font-size: 12px;">&copy; SecondBike — Used bicycle marketplace</p>
            </div>
            """;

        return (subject, body);
    }
}
