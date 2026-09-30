using JobApplication.Domain.Entities.Identity;
using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Repositories;

public interface IOtpRepository
{
    Task SetAsync(string id, OtpEntry entity, OtpPurpose otpPurpose, TimeSpan ttl, CancellationToken ck = default);

    Task<OtpEntry?> GetAsync(string id, OtpPurpose otpPurpose, CancellationToken ck = default);

    Task DeleteAsync(string id, OtpPurpose otpPurpose, CancellationToken ck = default);
}
