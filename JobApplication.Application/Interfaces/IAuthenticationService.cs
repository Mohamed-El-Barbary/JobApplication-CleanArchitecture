namespace JobApplication.Application.Interfaces;

using JobApplication.Application.DTOs.Authentication;
using System.Threading;
using System.Threading.Tasks;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}
