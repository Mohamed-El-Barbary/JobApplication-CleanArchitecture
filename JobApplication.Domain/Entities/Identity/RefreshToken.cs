using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities.Identity;

public class RefreshToken
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresOn { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public DateTime CreatedOn { get; set; }
    public DateTime? RevokeOn { get; set; }
    public bool IsActive => RevokeOn is null && !IsExpired;
}
