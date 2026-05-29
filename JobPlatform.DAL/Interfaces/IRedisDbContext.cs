using StackExchange.Redis;

namespace JobPlatform.DAL.Interfaces;

public interface IRedisDbContext
{
    Task<bool> AddAsync<T>(string imeiId, T value, TimeSpan lifeTime = default, When when = When.Always);
    Task<T?> GetAsync<T>(string imeiId, TimeSpan lifeTime = default);
}