namespace JobApplication.Application.Implementations;

using JobApplication.Application.DTOs.Authentication;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Options;
using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Entities.Identity;
using JobApplication.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<JwtOptions> jwtOptions,
    IUnitOfWork unitOfWork) : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private static readonly HashSet<string> AllowedPublicRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Candidate",
        "Recruiter"
    };

    public async Task<AuthenticationResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("FullName is required.", nameof(request.FullName));
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            throw new ArgumentException("A valid Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request.Password));
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Password and ConfirmPassword do not match.");
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new ArgumentException("Role is required.", nameof(request.Role));
        }

        if (!AllowedPublicRoles.Contains(request.Role) || string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Role '{request.Role}' is not allowed for public registration.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
        }

        string assignedRole = request.Role.Equals("Candidate", StringComparison.OrdinalIgnoreCase) ? "Candidate" : "Recruiter";

        if (!await _roleManager.RoleExistsAsync(assignedRole))
        {
            await _roleManager.CreateAsync(new ApplicationRole(assignedRole));
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, assignedRole);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Role assignment failed: {errors}");
        }

        // Create the corresponding business entity in the Business DB
        try
        {
            if (assignedRole == "Candidate")
            {
                var candidate = new Candidate
                {
                    FullName = user.FullName,
                    Email = user.Email!,
                    CVUrl = string.Empty,
                    UserId = user.Id
                };
                await _unitOfWork.Repository<Candidate>().AddAsync(candidate);
            }
            else
            {
                var recruiter = new Recruiter
                {
                    FullName = user.FullName,
                    Email = user.Email!,
                    UserId = user.Id
                };
                await _unitOfWork.Repository<Recruiter>().AddAsync(recruiter);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Compensating transaction: remove the Identity user to avoid orphaned records
            await _userManager.DeleteAsync(user);
            throw;
        }

        return await CreateAuthenticationResponseAsync(user, cancellationToken);
    }

    public async Task<AuthenticationResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request.Password));
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await CreateAuthenticationResponseAsync(user, cancellationToken);
    }

    public async Task<AuthenticationResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ArgumentException("RefreshToken is required.");
        }

        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == request.RefreshToken), cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var oldRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (oldRefreshToken == null || !oldRefreshToken.IsActive || oldRefreshToken.IsExpired || oldRefreshToken.RevokeOn != null)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        oldRefreshToken.RevokeOn = DateTime.UtcNow;

        return await CreateAuthenticationResponseAsync(user, cancellationToken);
    }

    private async Task<AuthenticationResponse> CreateAuthenticationResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        var accessToken = await GenerateAccessTokenAsync(user, roles, accessTokenExpiresAt);

        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        var refreshTokenString = GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenString,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = refreshTokenExpiresAt
        };

        user.RefreshTokens.Add(refreshTokenEntity);

        await _userManager.UpdateAsync(user);

        return new AuthenticationResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Roles = roles.ToList(),
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshTokenString,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    private Task<string> GenerateAccessTokenAsync(
        ApplicationUser user,
        IList<string> roles,
        DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
