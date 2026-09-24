namespace JobApplication.Application.Interfaces;

using JobApplication.Domain.Entities.Identity;
using System;
using System.Collections.Generic;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IList<string> roles);
    (string Token, DateTime ExpiresAt) GenerateRefreshToken();
}
