namespace JobApplication.Application.Interfaces;

using JobApplication.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendOtpEmailAsync(
            string to,
            string otp,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default);
}
