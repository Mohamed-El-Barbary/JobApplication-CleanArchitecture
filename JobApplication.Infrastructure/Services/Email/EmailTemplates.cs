using JobApplication.Domain.Enums;

namespace JobApplication.Infrastructure.Services.Email;

public static class EmailTemplates
{
    public static string OtpEmail(string otp, OtpPurpose purpose)
    {
        var title = purpose switch
        {
            OtpPurpose.EmailVerification => "Verify your email address",
            OtpPurpose.PasswordReset => "Reset your password",
            OtpPurpose.ChangeEmail => "Confirm your new email address",
            OtpPurpose.ChangePhoneNumber => "Verify your phone number",
            OtpPurpose.TwoFactorAuthentication => "Two-factor authentication",
            OtpPurpose.LoginVerification => "Login verification",
            _ => "Verification code"
        };

        return $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif;">

            <h2>{title}</h2>

            <p>
                Your verification code is:
            </p>

            <div style="
                font-size: 32px;
                font-weight: bold;
                letter-spacing: 8px;
                padding: 20px;
                margin: 20px 0;
                background-color: #f4f4f4;
                width: fit-content;">
                {otp}
            </div>

            <p>
                This code will expire in <strong>5 minutes</strong>.
            </p>

            <p>
                Do not share this code with anyone.
            </p>

            <p>
                If you did not request this code,
                you can safely ignore this email.
            </p>

        </body>
        </html>
        """;
    }
}
