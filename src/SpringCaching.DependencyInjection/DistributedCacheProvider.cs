using Microsoft.Extensions.Caching.Distributed;
using SpringCaching.Formatting;
using SpringCaching.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.DependencyInjection
{
    public class DistributedCacheProvider : ICacheProvider
    {
        public DistributedCacheProvider(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
#if NETSTANDARD2_0
            CacheSerializer = NewtonsoftJsonCacheSerializer.JsonCacheSerializer;    
#else
            CacheSerializer = SystemTextJsonCacheSerializer.JsonCacheSerializer;
#endif
        }
        private readonly IDistributedCache _distributedCache;
        public ICacheSerializer CacheSerializer { get; set; }
        public ValueTuple<bool, T?> Get<T>(string key)
        {
            var buffer = _distributedCache.Get(key);
            if (buffer == null)
            {
                return (false, default);
            }
            return (true, CacheSerializer.DeserializeObject<T>(buffer));
        }

        public async Task<ValueTuple<bool, T?>> GetAsync<T>(string key)
        {
            var buffer = await _distributedCache.GetAsync(key).ConfigureAwait(false);
            if (buffer == null)
            {
                return (false, default);
            }
            return (true, CacheSerializer.DeserializeObject<T>(buffer));
        }

        public void Set<T>(string key, T? value, TimeSpan? expirationTime)
        {
            var buffer = CacheSerializer.SerializeObject(value);
            _distributedCache.Set(key, buffer, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            });
        }

        public Task SetAsync<T>(string key, T? value, TimeSpan? expirationTime)
        {
            var buffer = CacheSerializer.SerializeObject(value);
            return _distributedCache.SetAsync(key, buffer, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            });
        }

        public void SetSliding<T>(string key, T? value, TimeSpan slidingExpirationTime)
        {
            var buffer = CacheSerializer.SerializeObject(value);
            _distributedCache.Set(key, buffer, new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpirationTime
            });
        }

        public Task SetSlidingAsync<T>(string key, T? value, TimeSpan slidingExpirationTime)
        {
            var buffer = CacheSerializer.SerializeObject(value);
            return _distributedCache.SetAsync(key, buffer, new DistributedCacheEntryOptions
            {
                SlidingExpiration = slidingExpirationTime
            });
        }

        public void Remove(string key)
        {
            _distributedCache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            return _distributedCache.RemoveAsync(key);
        }
    }
}
