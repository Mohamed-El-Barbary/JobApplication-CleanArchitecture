namespace JobApplication.Application.DTOs.Authentication;

using System;
using System.Collections.Generic;

/// <summary>
/// Intermediate DTO used to carry the user + resolved roles into the Mapster mapping pipeline.
/// </summary>
public class AuthUserDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public IList<string> Roles { get; set; } = [];
    public string AccessToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
