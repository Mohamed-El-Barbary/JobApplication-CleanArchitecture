namespace JobApplication.Infrastructure.Services.Email;

public static class EmailTemplates
{
    public static string VerificationEmail(string verificationLink)
    {
        return $@"
        <html>
        <body>
            <h2>Verify your email address</h2>
            <p>Thank you for registering with Job Application System!</p>
            <p>Please verify your email address by clicking the link below:</p>
            <p>
                <a href=""{verificationLink}"" style=""padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;"">Verify Email</a>
            </p>
            <p>This link will expire in 24 hours.</p>
            <p>If you did not request this, please ignore this email.</p>
        </body>
        </html>";
    }

    public static string PasswordResetEmail(string resetLink)
    {
        return $@"
        <html>
        <body>
            <h2>Reset your password</h2>
            <p>We received a request to reset your password for your Job Application System account.</p>
            <p>Click the link below to set a new password:</p>
            <p>
                <a href=""{resetLink}"" style=""padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;"">Reset Password</a>
            </p>
            <p>This link will expire in 2 hours.</p>
            <p>If you did not request a password reset, you can safely ignore this email.</p>
        </body>
        </html>";
    }
}
