using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAsync(OtpPurpose purpose, string id, string? ipAddress = null, CancellationToken ck = default);

    Task<bool> VerifyAsync(OtpPurpose purpose, string id, string otp, CancellationToken ck = default);

    Task RemoveAsync(OtpPurpose purpose, string id, CancellationToken ck = default);
}
