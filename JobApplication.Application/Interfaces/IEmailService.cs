namespace JobApplication.Application.Interfaces;

using System.Threading;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string to, string resetLink, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string to, string verificationLink, CancellationToken cancellationToken = default);
}
