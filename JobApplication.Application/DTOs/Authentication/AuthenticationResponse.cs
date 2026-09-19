namespace JobApplication.Application.DTOs.Authentication;

public class AuthenticationResponse
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public IReadOnlyCollection<string> Roles { get; set; }
        = Array.Empty<string>();

    public string AccessToken { get; set; } = null!;

    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = null!;

    public DateTime RefreshTokenExpiresAt { get; set; }
}
