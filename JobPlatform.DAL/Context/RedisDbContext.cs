using JobPlatform.DAL.Interfaces;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace JobPlatform.DAL.Context;

public class RedisDbContext(IRedisDatabase redisDatabase) : IRedisDbContext
{

    public Task<bool> AddAsync<T>(string key, T value, TimeSpan lifeTime = default, When when = When.Always)
    {
        return redisDatabase.AddAsync(key, value, lifeTime, when);
    }

    public Task<T?> GetAsync<T>(string key, TimeSpan lifeTime = default)
    {
        return redisDatabase.GetAsync<T>(key, lifeTime);
    }
}