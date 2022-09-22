using SpringCaching.Infrastructure;
using SpringCaching.Internal;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Proxy
{
    public static class SpringCachingProxyInvoker
    {
        #region Define
        internal static MethodInfo GetInvokeGenericMethod()
        {
            return typeof(SpringCachingProxyInvoker).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(o => o.IsGenericMethod && o.Name == "Invoke").FirstOrDefault()!;
        }

        internal static MethodInfo GetInvokeAsyncGenericMethod()
        {
            return typeof(SpringCachingProxyInvoker).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(o => o.IsGenericMethod && o.Name == "InvokeAsync").FirstOrDefault()!;
        }

        internal static MethodInfo GetInvokeMethod()
        {
            return typeof(SpringCachingProxyInvoker).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(o => !o.IsGenericMethod && o.Name == "Invoke").FirstOrDefault()!;
        }

        internal static MethodInfo GetInvokeAsyncMethod()
        {
            return typeof(SpringCachingProxyInvoker).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(o => !o.IsGenericMethod && o.Name == "InvokeAsync").FirstOrDefault()!;
        }
        #endregion

        /// <summary>
        /// async invoke proxy
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// <para><see cref="ICacheableRequirement"/></para>
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="requirement"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        public static async Task InvokeAsync(ISpringCachingProxyContext context, Func<Task> invoker)
        {
            var cacheEvictRequirements = context.Requirement.GetCacheEvictRequirements() ?? ArrayEx.Empty<ICacheEvictRequirement>();
            foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
            {
                InvokeCacheEvict(context, cacheEvictRequirement);
            }
            await invoker().ConfigureAwait(false);
            foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => !s.BeforeInvocation))
            {
                InvokeCacheEvict(context, cacheEvictRequirement);
            }
        }

        /// <summary>
        /// async invoke proxy
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// <para><see cref="ICacheableRequirement"/></para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="proxy"></param>
        /// <param name="requirement"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        public static async Task<TResult?> InvokeAsync<TResult>(ISpringCachingProxyContext context, Func<Task<TResult?>> invoker)
        {
            var cacheEvictRequirements = context.Requirement.GetCacheEvictRequirements();
            var cacheableRequirements = context.Requirement.GetCacheableRequirements();

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
                {
                    await InvokeCacheEvictAsync(context, cacheEvictRequirement).ConfigureAwait(false);
                }
            }

            var invokeValue = cacheableRequirements == null ?
             await invoker().ConfigureAwait(false)
             : await InvokeCacheableAsync(context, cacheableRequirements, invoker).ConfigureAwait(false);

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => !s.BeforeInvocation))
                {
                    await InvokeCacheEvictAsync(context, cacheEvictRequirement).ConfigureAwait(false);
                }
            }

            return invokeValue;
        }

        /// <summary>
        /// invoke proxy
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="requirement"></param>
        /// <param name="invoker"></param>
        public static void Invoke(ISpringCachingProxyContext context, Action invoker)
        {
            var cacheEvictRequirements = context.Requirement.GetCacheEvictRequirements() ?? ArrayEx.Empty<ICacheEvictRequirement>();
            foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
            {
                InvokeCacheEvict(context, cacheEvictRequirement);
            }
            invoker();
            foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => !s.BeforeInvocation))
            {
                InvokeCacheEvict(context, cacheEvictRequirement);
            }
        }
        /// <summary>
        /// invoke proxy
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="proxy"></param>
        /// <param name="requirement"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        public static TResult? Invoke<TResult>(ISpringCachingProxyContext context, Func<TResult?> invoker)
        {
            var cacheEvictRequirements = context.Requirement.GetCacheEvictRequirements();
            var cacheableRequirements = context.Requirement.GetCacheableRequirements();

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
                {
                    InvokeCacheEvict(context, cacheEvictRequirement);
                }
            }

            var invokeValue = cacheableRequirements == null ?
             invoker()
             : InvokeCacheable(context, cacheableRequirements, invoker);

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => !s.BeforeInvocation))
                {
                    InvokeCacheEvict(context, cacheEvictRequirement);
                }
            }

            return invokeValue;
        }


        #region invoke Cacheable
        private static async Task<TResult?> InvokeCacheableAsync<TResult>(ISpringCachingProxyContext context, IList<ICacheableRequirement> cacheableRequirements, Func<Task<TResult?>> invoker)
        {
            var proxy = context.Proxy;
            var requirement = context.Requirement;
            string[] keys = new string[cacheableRequirements.Count];
            int index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                string key = GetCacheKey(cacheableRequirement.Value, cacheableRequirement.Key, cacheableRequirement.KeyGenerator, requirement);
                //cache key
                keys[index] = key;
                var cacheResult = await proxy.CacheProvider.GetAsync<TResult>(key).ConfigureAwait(false);
                if (cacheResult.Item1)
                {
                    //if has cache , return it
                    return cacheResult.Item2;
                }
            }
            var invokerValue = await invoker().ConfigureAwait(false);

            //cache value
            index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                if (!IsCacheResult(cacheableRequirement, requirement, invokerValue))
                {
                    continue;
                }
                //cache it
                await SetCacheAsync(proxy.CacheProvider, keys[index]!, invokerValue, cacheableRequirement).ConfigureAwait(false);
            }
            return invokerValue;
        }

        private static TResult? InvokeCacheable<TResult>(ISpringCachingProxyContext context, IList<ICacheableRequirement> cacheableRequirements, Func<TResult?> invoker)
        {
            var proxy = context.Proxy;
            var requirement = context.Requirement;
            string[] keys = new string[cacheableRequirements.Count];
            int index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                string key = GetCacheKey(cacheableRequirement.Value, cacheableRequirement.Key, cacheableRequirement.KeyGenerator, requirement);
                //cache key
                keys[index] = key;
                var cacheResult = proxy.CacheProvider.Get<TResult>(key);
                if (cacheResult.Item1)
                {
                    //if has cache , return it
                    return cacheResult.Item2;
                }
            }
            var invokerValue = invoker();

            //cache value
            index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                if (!IsCacheResult(cacheableRequirement,requirement, invokerValue))
                {
                    continue;
                }
                //cache it
                SetCache(proxy.CacheProvider, keys[index]!, invokerValue, cacheableRequirement);
            }
            return invokerValue;
        }
        #endregion

        #region invoke CacheEvict
        private static Task InvokeCacheEvictAsync(ISpringCachingProxyContext context, ICacheEvictRequirement cacheEvictRequirement)
        {
            if (cacheEvictRequirement == null)
            {
                return TaskEx.CompletedTask;
            }
            string key = cacheEvictRequirement.AllEntries ?
                cacheEvictRequirement.Value :
                GetCacheKey(cacheEvictRequirement.Value, cacheEvictRequirement.Key, cacheEvictRequirement.KeyGenerator, context.Requirement);
            return context.Proxy.CacheProvider.RemoveAsync(key);
        }

        private static void InvokeCacheEvict(ISpringCachingProxyContext context, ICacheEvictRequirement cacheEvictRequirement)
        {
            if (cacheEvictRequirement == null)
            {
                return;
            }
            string key = cacheEvictRequirement.AllEntries ?
                cacheEvictRequirement.Value :
                GetCacheKey(cacheEvictRequirement.Value, cacheEvictRequirement.Key, cacheEvictRequirement.KeyGenerator, context.Requirement);
            context.Proxy.CacheProvider.Remove(key);
        }
        #endregion

        //        #region invoke CachePut
        //        private static async
        //#if NET45
        //Task<Tuple<bool, TResult>>
        //#else
        //Task<ValueTuple<bool, TResult>>
        //#endif
        //            InvokeCachePutAsync<TResult>(ISpringCachingProxyContext context, IList<ICachePutRequirement> cachePutRequirements, Func<Task<TResult?>> invoker)
        //        {
        //            //if (cachePutRequirements == null)
        //            //{
        //            //    return false;
        //            //}
        //            var proxy = context.Proxy;
        //            var requirement = context.Requirement;
        //            foreach (var cachePutRequirement in cachePutRequirements)
        //            {
        //                if (cachePutRequirement == null)
        //                {
        //                    continue;
        //                }
        //                string key = GetCacheKey(cachePutRequirement.Value, cachePutRequirement.Key, cachePutRequirement.KeyGenerator, requirement);
        //            }
        //            var invokerValue = await invoker().ConfigureAwait(false);

        //            //cache value
        //            index = -1;
        //            foreach (var cacheableRequirement in cacheableRequirements)
        //            {
        //                index++;
        //                if (cacheableRequirement == null)
        //                {
        //                    continue;
        //                }
        //                if (cacheableRequirement.UnlessNull && invokerValue == null)
        //                {
        //                    //if result is null, don't cache it
        //                    continue;
        //                }
        //                if (!IsPredicate(cacheableRequirement.Condition, cacheableRequirement.ConditionGenerator, requirement))
        //                {
        //                    //don't cache
        //                    continue;
        //                }
        //                //cache it
        //                await SetCacheAsync(proxy.CacheProvider, keys[index]!, invokerValue, cacheableRequirement).ConfigureAwait(false);
        //            }
        //            return invokerValue;
        //        }

        //        private static TResult? InvokeCachePut<TResult>(ISpringCachingProxyContext context, IList<ICacheableRequirement> cachePutRequirements, Func<TResult?> invoker)
        //        {
        //            var proxy = context.Proxy;
        //            var requirement = context.Requirement;
        //            foreach (var cachePutRequirement in cachePutRequirements)
        //            {
        //                if (cachePutRequirement == null)
        //                {
        //                    continue;
        //                }
        //                string key = GetCacheKey(cacheableRequirement.Value, cacheableRequirement.Key, cacheableRequirement.KeyGenerator, requirement);
        //                //cache key
        //                keys[index] = key;
        //                var cacheResult = proxy.CacheProvider.Get<TResult>(key);
        //                if (cacheResult.Item1)
        //                {
        //                    //if has cache , return it
        //                    return cacheResult.Item2;
        //                }
        //            }
        //            var invokerValue = invoker();

        //            //cache value
        //            index = -1;
        //            foreach (var cacheableRequirement in cacheableRequirements)
        //            {
        //                index++;
        //                if (cacheableRequirement == null)
        //                {
        //                    continue;
        //                }
        //                if (cacheableRequirement.UnlessNull && invokerValue == null)
        //                {
        //                    //if result is null, don't cache it
        //                    continue;
        //                }
        //                if (!IsPredicate(cacheableRequirement.Condition, cacheableRequirement.ConditionGenerator, requirement))
        //                {
        //                    //don't cache
        //                    continue;
        //                }
        //                //cache it
        //                SetCache(proxy.CacheProvider, keys[index]!, invokerValue, cacheableRequirement);
        //            }
        //            return invokerValue;
        //        }
        //        #endregion

        private static Task SetCacheAsync<TResult>(ICacheProvider cacheProvider, string key, TResult value, ICacheableRequirement cacheableRequirement)
        {

            switch (cacheableRequirement.ExpirationPolicy)
            {
                case ExpirationPolicy.Absolute:
                    return cacheProvider.SetAsync(key, value, GetExpirationTime(cacheableRequirement));
                case ExpirationPolicy.Sliding:
                    var expirationTime = GetExpirationTime(cacheableRequirement);
                    if (expirationTime.HasValue)
                    {
                        return cacheProvider.SetSlidingAsync(key, value, expirationTime.Value);
                    }
                    return cacheProvider.SetAsync(key, value, expirationTime);
                default:
                    return cacheProvider.SetAsync(key, value, null);
            }
        }

        private static void SetCache<TResult>(ICacheProvider cacheProvider, string key, TResult value, ICacheableRequirement cacheableRequirement)
        {
            switch (cacheableRequirement.ExpirationPolicy)
            {
                case ExpirationPolicy.Absolute:
                    cacheProvider.Set(key, value, GetExpirationTime(cacheableRequirement));
                    return;
                case ExpirationPolicy.Sliding:
                    var expirationTime = GetExpirationTime(cacheableRequirement);
                    if (expirationTime.HasValue)
                    {
                        cacheProvider.SetSliding(key, value, expirationTime.Value);
                        return;
                    }
                    cacheProvider.Set(key, value, expirationTime);
                    return;
                default:
                    cacheProvider.Set(key, value, null);
                    return;
            }
        }

        private static string GetCacheKey(string cacheableValue, string? cacheableKey)
        {
            if (string.IsNullOrWhiteSpace(cacheableKey))
            {
                return cacheableValue;
            }
            return $"{cacheableValue}:{cacheableKey}";
        }

        private static string GetCacheKey(string cacheableValue, string? key, IKeyGenerator? keyGenerator, ISpringCachingRequirement requirement)
        {
            return GetCacheKey(cacheableValue, keyGenerator?.GetKey(key, requirement));
        }

        private static bool IsPredicate(string? expression, IPredicateGenerator? predicateGenerator, ISpringCachingRequirement requirement)
        {
            if (predicateGenerator == null)
            {
                return true;
            }
            return predicateGenerator.Predicate(expression, requirement);
        }

        private static bool IsCacheResult<TResult>(ICacheableRequirement cacheableRequirement, ISpringCachingRequirement requirement, TResult? result)
        {
            if (cacheableRequirement.UnlessNull && result == null)
            {
                //if result is null, don't cache it
                return false;
            }
            if (!IsPredicate(cacheableRequirement.Condition, cacheableRequirement.ConditionGenerator, requirement))
            {
                //don't cache
                return false;
            }
            return true;
        }

        private static TimeSpan? GetExpirationTime(ICacheableRequirement cacheableRequirement)
        {
            if (cacheableRequirement.ExpirationPolicy == ExpirationPolicy.None)
            {
                return null;
            }
            if (cacheableRequirement.ExpirationValue <= 0)
            {
                return null;
            }
            return cacheableRequirement.ExpirationUnit switch
            {
                ExpirationUnit.Second => TimeSpan.FromSeconds(cacheableRequirement.ExpirationValue),
                ExpirationUnit.Minute => TimeSpan.FromMinutes(cacheableRequirement.ExpirationValue),
                ExpirationUnit.Hour => TimeSpan.FromHours(cacheableRequirement.ExpirationValue),
                ExpirationUnit.Day => TimeSpan.FromDays(cacheableRequirement.ExpirationValue),
                _ => null,
            };
        }


    }
}
