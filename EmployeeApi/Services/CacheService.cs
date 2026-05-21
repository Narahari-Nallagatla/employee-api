using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace EmployeeApi.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IDistributedCache cache,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);

                if (string.IsNullOrEmpty(data))
                    return default;

                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Redis GET failed for key {Key}", key);

                return default; // fallback
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            try
            {
                var data = JsonSerializer.Serialize(value);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                };

                await _cache.SetStringAsync(key, data, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis SET failed for key {Key}", key);
            }
        }
    }
}
