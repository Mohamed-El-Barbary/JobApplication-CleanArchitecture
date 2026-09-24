namespace JobApplication.Application.Interfaces;

using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IIdentityService
{
    // --- Registration & Lookup ---
    Task<Result<ApplicationUser>> RegisterUserAsync(string email, string password, string fullName, string role, CancellationToken cancellationToken = default);
    Task<Result<ApplicationUser>> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<ApplicationUser>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ApplicationUser>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<ApplicationUser>> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<IList<string>>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    // --- Refresh Tokens ---
    Task<Result> RevokeRefreshTokenAsync(ApplicationUser user, string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> RevokeAllRefreshTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<Result> AddRefreshTokenAsync(ApplicationUser user, RefreshToken refreshTokenEntity, CancellationToken cancellationToken = default);

    // --- Password ---
    Task<Result> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<Result<string>> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken cancellationToken = default);

    // --- Email Confirmation ---
    Task<Result<string>> GenerateEmailConfirmationTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<Result> ConfirmEmailAsync(ApplicationUser user, string token, CancellationToken cancellationToken = default);
    Task<bool> IsEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}
