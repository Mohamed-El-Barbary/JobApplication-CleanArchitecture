namespace JobApplication.Infrastructure.Services;

using JobApplication.Application.Interfaces;
using JobApplication.Domain.Common;
using JobApplication.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IIdentityService
{
    private static readonly HashSet<string> AllowedPublicRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Candidate",
        "Recruiter"
    };

    public async Task<Result<ApplicationUser>> RegisterUserAsync(string email, string password, string fullName, string role, CancellationToken cancellationToken = default)
    {
        if (!AllowedPublicRoles.Contains(role) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation("Identity.InvalidRole", $"Role '{role}' is not allowed for public registration.");
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Error.Conflict("Identity.UserExists", $"User with email '{email}' already exists.");
        }

        string assignedRole = role.Equals("Candidate", StringComparison.OrdinalIgnoreCase) ? "Candidate" : "Recruiter";

        if (!await roleManager.RoleExistsAsync(assignedRole))
        {
            await roleManager.CreateAsync(new ApplicationRole(assignedRole));
        }

        var user = new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            UserName = email
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Error.Failure("Identity.CreationFailed", $"User creation failed: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, assignedRole);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            return Error.Failure("Identity.RoleAssignmentFailed", $"Role assignment failed: {errors}");
        }

        return user;
    }

    public async Task<Result<ApplicationUser>> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Error.Unauthorized("Identity.InvalidCredentials", "Invalid email or password.");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            return Error.Unauthorized("Identity.InvalidCredentials", "Invalid email or password.");
        }

        return user;
    }

    public async Task<Result<ApplicationUser>> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken), cancellationToken);

        if (user == null)
        {
            return Error.Unauthorized("Identity.InvalidToken", "Invalid refresh token.");
        }

        return user;
    }

    public async Task<Result<IList<string>>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await userManager.GetRolesAsync(user);
        return Result<IList<string>>.Success(roles);
    }

    public async Task<Result> RevokeRefreshTokenAsync(ApplicationUser user, string refreshToken, CancellationToken cancellationToken = default)
    {
        var oldRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (oldRefreshToken == null || !oldRefreshToken.IsActive || oldRefreshToken.IsExpired || oldRefreshToken.RevokeOn != null)
        {
            return Result.Failure(Error.Unauthorized("Identity.TokenExpiredOrRevoked", "Refresh token is expired or revoked."));
        }

        oldRefreshToken.RevokeOn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> AddRefreshTokenAsync(ApplicationUser user, RefreshToken refreshTokenEntity, CancellationToken cancellationToken = default)
    {
        user.RefreshTokens.Add(refreshTokenEntity);
        var result = await userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            return Result.Failure(Error.Failure("Identity.UpdateFailed", "Failed to add refresh token."));
        }
        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
             return Result.Failure(Error.Failure("Identity.DeleteFailed", "Failed to delete user."));
        }
        return Result.Success();
    }

    public async Task<Result<ApplicationUser>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Error.NotFound("Identity.UserNotFound", "User not found.");

        return user;
    }

    public async Task<Result<ApplicationUser>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
            return Error.NotFound("Identity.UserNotFound", "User not found.");

        return user;
    }

    public async Task<Result> RevokeAllRefreshTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
        {
            token.RevokeOn = DateTime.UtcNow;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Failure(Error.Failure("Identity.UpdateFailed", "Failed to revoke refresh tokens."));

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Failure("Identity.ChangePasswordFailed", errors));
        }

        return Result.Success();
    }

    public async Task<Result<string>> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return token;
    }

    public async Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Failure("Identity.ResetPasswordFailed", errors));
        }

        return Result.Success();
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return token;
    }

    public async Task<Result> ConfirmEmailAsync(ApplicationUser user, string token, CancellationToken cancellationToken = default)
    {
        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Failure("Identity.EmailConfirmationFailed", errors));
        }

        return Result.Success();
    }

    public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return await userManager.IsEmailConfirmedAsync(user);
    }
}
