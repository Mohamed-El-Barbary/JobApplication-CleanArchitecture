namespace JobApplication.Application.DTOs.Authentication;

public class RevokeRefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}
