using SpringCaching.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching
{
    public class EmptyCacheProvider : ICacheProvider
    {
#if NET45
        public Tuple<bool, T?> Get<T>(string key)
        {
            return Tuple.Create<bool, T?>(false, default);
        }
#else
        public ValueTuple<bool, T?> Get<T>(string key)
        {
            return (false, default);
        }
#endif


#if NET45
        public Task<Tuple<bool, T?>> GetAsync<T>(string key)
        {
            return Task.FromResult(Tuple.Create<bool, T?>(false, default));
        }
#else
        public Task<ValueTuple<bool, T?>> GetAsync<T>(string key)
        {
            return Task.FromResult(ValueTuple.Create<bool, T?>(false, default));
        }
#endif

        public void Set<T>(string key, T? value, TimeSpan? expirationTime)
        {
        }

        public void SetSliding<T>(string key, T? value, TimeSpan slidingExpirationTime)
        {
        }

        public Task SetAsync<T>(string key, T? value, TimeSpan? expirationTime)
        {
            return TaskEx.CompletedTask;
        }



        public Task SetSlidingAsync<T>(string key, T? value, TimeSpan slidingExpirationTime)
        {
            return TaskEx.CompletedTask;
        }

        public void Remove(string key)
        {
        }

        public Task RemoveAsync(string key)
        {
            return TaskEx.CompletedTask;
        }

        public void Clear(string key)
        {
        }

        public Task ClearAsync(string key)
        {
            return TaskEx.CompletedTask;
        }

    }
}
