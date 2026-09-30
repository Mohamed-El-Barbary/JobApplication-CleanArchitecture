using JobApplication.Domain.Entities.Identity;
using JobApplication.Domain.Enums;
using JobApplication.Domain.Repositories;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace JobApplication.Infrastructure.Repositories;

public sealed class RedisOtpRepository(IConnectionMultiplexer redis) : IOtpRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    public async Task SetAsync(string id, OtpEntry entity, OtpPurpose otpPurpose, TimeSpan ttl, CancellationToken ck = default)
    {
        var key = BuildKey(otpPurpose, id);

        var json = JsonSerializer.Serialize(entity);

        await _database.StringSetAsync(key, json, ttl);
    }

    public async Task<OtpEntry?> GetAsync(string id, OtpPurpose otpPurpose, CancellationToken ck = default)
    {
        var key = BuildKey(otpPurpose, id);
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<OtpEntry>(value.ToString());
    }

    public async Task DeleteAsync(string id, OtpPurpose otpPurpose, CancellationToken ck = default)
    {
        var key = BuildKey(otpPurpose, id);
        await _database.KeyDeleteAsync(key);
    }


    private static string BuildKey(OtpPurpose purpose, string id)
        => $"otp:{purpose}:{id.Trim().ToLowerInvariant()}";
}
