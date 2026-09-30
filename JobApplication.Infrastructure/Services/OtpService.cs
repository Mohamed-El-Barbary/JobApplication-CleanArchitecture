using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities.Identity;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace JobApplication.Infrastructure.Services;

public sealed class OtpService(IOtpRepository otpRepository) : IOtpService
{
    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(5);
    private const int MaxAttempts = 5;


    public async Task<string> GenerateAsync(OtpPurpose purpose, string id, string? ipAddress = null, CancellationToken ck = default)
    {
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var date = DateTime.UtcNow;

        var entry = new OtpEntry
        {
            HashedCode = HashOtp(otp),
            CreatedAt = date,
            ExpiresAt = date.Add(OtpExpiration),
            IpAddress = ipAddress,
            AttemptCount = 0,
            Purpose = purpose
        };

        await otpRepository.SetAsync(
            id,
            entry,
            purpose,
            OtpExpiration,
            ck);

        return otp; throw new NotImplementedException();
    }

    public async Task<bool> VerifyAsync(OtpPurpose purpose, string id, string otp, CancellationToken ck = default)
    {
        var entry = await otpRepository.GetAsync(id, purpose, ck);

        if (entry is null)
            return false;

        if (entry.ExpiresAt <= DateTime.UtcNow)
        {
            await otpRepository.DeleteAsync(id, purpose, ck);
            return false;
        }

        if (entry.AttemptCount >= MaxAttempts)
        {
            await otpRepository.DeleteAsync(id, purpose, ck);
            return false;
        }

        entry.AttemptCount++;

        var hashedOtp = HashOtp(otp);

        var isValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(entry.HashedCode),
            Encoding.UTF8.GetBytes(hashedOtp));

        if (!isValid)
        {
            // Store updated attempt count.
            var remainingTime = entry.ExpiresAt - DateTime.UtcNow;

            if (remainingTime <= TimeSpan.Zero)
                await otpRepository.DeleteAsync(id, purpose, ck);
            else
                await otpRepository.SetAsync(id, entry, purpose, remainingTime, ck);

            return false;
        }

        // OTP is one-time use.
        await otpRepository.DeleteAsync(id, purpose, ck);

        return true;
    }

    public async Task RemoveAsync(OtpPurpose purpose, string id, CancellationToken ck = default)
    {
        await otpRepository.DeleteAsync(id, purpose, ck);
    }

    private static string HashOtp(string otp)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(otp));

        return Convert.ToHexString(hash);
    }
}
