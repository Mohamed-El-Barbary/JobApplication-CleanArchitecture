namespace JobApplication.Infrastructure.Services.Email;

using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ResendEmailService(
    IResend resend,
    IConfiguration configuration,
    ILogger<ResendEmailService> logger) : IEmailService
{
    public async Task SendOtpEmailAsync(string to, string otp, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        var subject = purpose switch
        {
            OtpPurpose.EmailVerification => "Verify your email address",
            OtpPurpose.PasswordReset => "Reset your password",
            OtpPurpose.ChangeEmail => "Confirm your new email address",
            OtpPurpose.ChangePhoneNumber => "Verify your phone number",
            OtpPurpose.TwoFactorAuthentication => "Your verification code",
            OtpPurpose.LoginVerification => "Your login verification code",
            _ => "Your verification code"
        };

        var htmlBody = EmailTemplates.OtpEmail(otp,purpose);

        await SendEmailInternalAsync(
            to,
            subject,
            htmlBody,
            cancellationToken);
    }

    private async Task SendEmailInternalAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var fromEmail = configuration["Resend:FromEmail"];
        var fromName = configuration["Resend:FromName"];

        if (string.IsNullOrEmpty(fromEmail))
        {
            logger.LogWarning("Resend:FromEmail is not configured.");
            return;
        }

        var message = new EmailMessage
        {
            From = $"{fromName} <{fromEmail}>",
            To = { to },
            Subject = subject,
            HtmlBody = htmlBody
        };

        try
        {
            await resend.EmailSendAsync(message, cancellationToken);
            logger.LogInformation("Email sent successfully to {ToEmail}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {ToEmail}", to);
            throw;
        }
    }
}
