using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities.Identity;

public sealed class OtpEntry
{
    public string HashedCode { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string? IpAddress { get; set; }

    public int AttemptCount { get; set; }

    public OtpPurpose Purpose { get; set; }
}
