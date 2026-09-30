using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Enums;

public enum OtpPurpose
{
    EmailVerification = 1,
    PasswordReset = 2,
    ChangeEmail = 3,
    ChangePhoneNumber = 4,
    TwoFactorAuthentication = 5,
    LoginVerification = 6
}
