using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching
{
    public interface ICacheProvider
    {
        /// <summary>
        /// get cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
#if NET45
        Tuple<bool, T?> Get<T>(string key);
#else
        ValueTuple<bool, T?> Get<T>(string key);
#endif
        /// <summary>
        /// get cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
#if NET45
        Task<Tuple<bool, T?>> GetAsync<T>(string key);
#else
        Task<ValueTuple<bool, T?>> GetAsync<T>(string key);
#endif
        /// <summary>
        /// set cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime">失效时间</param>
        void Set<T>(string key, T? value, TimeSpan? expirationTime);
        /// <summary>
        /// set cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime">失效时间</param>
        /// <returns></returns>
        Task SetAsync<T>(string key, T? value, TimeSpan? expirationTime);
        /// <summary>
        /// set sliding cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingExpirationTime">滑动失效时间</param>
        void SetSliding<T>(string key, T? value, TimeSpan slidingExpirationTime);
        /// <summary>
        /// set sliding cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingExpirationTime">滑动失效时间</param>
        Task SetSlidingAsync<T>(string key, T? value, TimeSpan slidingExpirationTime);
        /// <summary>
        /// remove cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        void Remove(string key);
        /// <summary>
        /// remove cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveAsync(string key);
    }
}
