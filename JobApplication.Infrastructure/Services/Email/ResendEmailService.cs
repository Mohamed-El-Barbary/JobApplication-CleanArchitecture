namespace JobApplication.Infrastructure.Services.Email;

using JobApplication.Application.Interfaces;
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
    public async Task SendPasswordResetEmailAsync(string to, string resetLink, CancellationToken cancellationToken = default)
    {
        var htmlBody = EmailTemplates.PasswordResetEmail(resetLink);
        await SendEmailInternalAsync(to, "Reset your password", htmlBody, cancellationToken);
    }

    public async Task SendVerificationEmailAsync(string to, string verificationLink, CancellationToken cancellationToken = default)
    {
        var htmlBody = EmailTemplates.VerificationEmail(verificationLink);
        await SendEmailInternalAsync(to, "Verify your email address", htmlBody, cancellationToken);
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
