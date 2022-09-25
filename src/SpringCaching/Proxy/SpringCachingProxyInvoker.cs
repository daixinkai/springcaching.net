using SpringCaching.Infrastructure;
using SpringCaching.Infrastructure.Parser;
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
        /// invoke proxy
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invoker"></param>
        public static void Invoke(ISpringCachingProxyContext context, Action invoker)
        {
            // Action has no result,so not support ICacheableRequirement and ICachePutRequirement
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
        /// async invoke proxy
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        public static async Task InvokeAsync(ISpringCachingProxyContext context, Func<Task> invoker)
        {
            // Func<Task> has no result,so not support ICacheableRequirement and ICachePutRequirement
            var cacheEvictRequirements = context.Requirement.GetCacheEvictRequirements() ?? ArrayEx.Empty<ICacheEvictRequirement>();
            foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
            {
                await InvokeCacheEvictAsync(context, cacheEvictRequirement).ConfigureAwait(false);
            }
            await invoker().ConfigureAwait(false);
            foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => !s.BeforeInvocation))
            {
                await InvokeCacheEvictAsync(context, cacheEvictRequirement).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// invoke proxy
        /// <para><see cref="ICacheableRequirement"/></para>
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// <para><see cref="ICachePutRequirement"/></para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        public static TResult? Invoke<TResult>(ISpringCachingProxyContext context, Func<TResult?> invoker)
        {
            var funcInvoker = new FuncInvoker<TResult>(invoker);
            var cacheEvictRequirements = context.Requirement.GetCacheEvictRequirements();
            var cacheableRequirements = context.Requirement.GetCacheableRequirements();
            var cachePutRequirements = context.Requirement.GetCachePutRequirements();

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
                {
                    InvokeCacheEvict(context, cacheEvictRequirement);
                }
            }

            if (cachePutRequirements != null)
            {
                InvokeCachePut(context, cachePutRequirements, funcInvoker);
            }

            var invokeValue = cacheableRequirements == null ?
             funcInvoker.GetResult()
             : InvokeCacheable(context, cacheableRequirements, funcInvoker);

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => !s.BeforeInvocation))
                {
                    InvokeCacheEvict(context, cacheEvictRequirement);
                }
            }

            return invokeValue;
        }

        /// <summary>
        /// async invoke proxy
        /// <para><see cref="ICacheableRequirement"/></para>
        /// <para><see cref="ICacheEvictRequirement"/></para>
        /// <para><see cref="ICachePutRequirement"/></para>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="context"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        public static async Task<TResult?> InvokeAsync<TResult>(ISpringCachingProxyContext context, Func<Task<TResult?>> invoker)
        {
            var funcInvoker = new AsyncFuncInvoker<TResult>(invoker);
            var cacheEvictRequirements = context.Requirement.GetCacheEvictRequirements();
            var cacheableRequirements = context.Requirement.GetCacheableRequirements();
            var cachePutRequirements = context.Requirement.GetCachePutRequirements();

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
                {
                    await InvokeCacheEvictAsync(context, cacheEvictRequirement).ConfigureAwait(false);
                }
            }

            if (cachePutRequirements != null)
            {
                await InvokeCachePutAsync(context, cachePutRequirements, funcInvoker).ConfigureAwait(false);
            }

            var invokeValue = cacheableRequirements == null ?
             await funcInvoker.GetResultAsync().ConfigureAwait(false)
             : await InvokeCacheableAsync(context, cacheableRequirements, funcInvoker).ConfigureAwait(false);

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => !s.BeforeInvocation))
                {
                    await InvokeCacheEvictAsync(context, cacheEvictRequirement).ConfigureAwait(false);
                }
            }

            return invokeValue;
        }


        #region invoke Cacheable
        private static TResult? InvokeCacheable<TResult>(ISpringCachingProxyContext context, IList<ICacheableRequirement> cacheableRequirements, FuncInvoker<TResult> invoker)
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
                string key = GetCacheKey(cacheableRequirement, proxy, requirement);
                //cache key
                keys[index] = key;
                var cacheResult = proxy.CacheProvider.Get<TResult>(key);
                if (cacheResult.Item1)
                {
                    //if has cache , return it
                    return cacheResult.Item2;
                }
            }
            var invokerValue = invoker.GetResult();
            //cache value
            index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                if (!IsCacheResult(cacheableRequirement, proxy, requirement, invokerValue))
                {
                    continue;
                }
                //cache it
                SetCache(proxy.CacheProvider, keys[index]!, invokerValue, cacheableRequirement);
            }
            return invokerValue;
        }
        private static async Task<TResult?> InvokeCacheableAsync<TResult>(ISpringCachingProxyContext context, IList<ICacheableRequirement> cacheableRequirements, AsyncFuncInvoker<TResult> invoker)
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
                string key = GetCacheKey(cacheableRequirement, proxy, requirement);
                //cache key
                keys[index] = key;
                var cacheResult = await proxy.CacheProvider.GetAsync<TResult>(key).ConfigureAwait(false);
                if (cacheResult.Item1)
                {
                    //if has cache , return it
                    return cacheResult.Item2;
                }
            }
            var invokerValue = await invoker.GetResultAsync().ConfigureAwait(false);

            //cache value
            index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                if (!IsCacheResult(cacheableRequirement, proxy, requirement, invokerValue))
                {
                    continue;
                }
                //cache it
                await SetCacheAsync(proxy.CacheProvider, keys[index]!, invokerValue, cacheableRequirement).ConfigureAwait(false);
            }
            return invokerValue;
        }
        #endregion

        #region invoke CacheEvict
        private static void InvokeCacheEvict(ISpringCachingProxyContext context, ICacheEvictRequirement cacheEvictRequirement)
        {
            if (cacheEvictRequirement == null)
            {
                return;
            }
            if (cacheEvictRequirement.AllEntries)
            {
                context.Proxy.CacheProvider.Clear(cacheEvictRequirement.Value);
            }
            else
            {
                context.Proxy.CacheProvider.Remove(GetCacheKey(cacheEvictRequirement, context.Proxy, context.Requirement));
            }
        }
        private static Task InvokeCacheEvictAsync(ISpringCachingProxyContext context, ICacheEvictRequirement cacheEvictRequirement)
        {
            if (cacheEvictRequirement == null)
            {
                return TaskEx.CompletedTask;
            }
            if (cacheEvictRequirement.AllEntries)
            {
                return context.Proxy.CacheProvider.ClearAsync(cacheEvictRequirement.Value);
            }
            else
            {
                return context.Proxy.CacheProvider.RemoveAsync(GetCacheKey(cacheEvictRequirement, context.Proxy, context.Requirement));
            }
        }

        #endregion

        #region invoke CachePut
        private static void InvokeCachePut<TResult>(ISpringCachingProxyContext context, IList<ICachePutRequirement> cachePutRequirements, FuncInvoker<TResult> invoker)
        {
            var proxy = context.Proxy;
            var requirement = context.Requirement;
            var invokerValue = invoker.GetResult();
            foreach (var cachePutRequirement in cachePutRequirements)
            {
                if (cachePutRequirement == null)
                {
                    continue;
                }
                if (!IsCacheResult(cachePutRequirement, proxy, requirement, invokerValue))
                {
                    //don't cache
                    continue;
                }
                //cache it
                string key = GetCacheKey(cachePutRequirement, proxy, requirement);
                SetCache(proxy.CacheProvider, key, invokerValue, cachePutRequirement);
            }
        }
        private static async Task InvokeCachePutAsync<TResult>(ISpringCachingProxyContext context, IList<ICachePutRequirement> cachePutRequirements, AsyncFuncInvoker<TResult> invoker)
        {
            var proxy = context.Proxy;
            var requirement = context.Requirement;
            var invokerValue = await invoker.GetResultAsync().ConfigureAwait(false);
            foreach (var cachePutRequirement in cachePutRequirements)
            {
                if (cachePutRequirement == null)
                {
                    continue;
                }
                if (!IsCacheResult(cachePutRequirement, proxy, requirement, invokerValue))
                {
                    //don't cache
                    continue;
                }
                //cache it
                string key = GetCacheKey(cachePutRequirement, proxy, requirement);
                await SetCacheAsync(proxy.CacheProvider, key, invokerValue, cachePutRequirement).ConfigureAwait(false);
            }
        }

        #endregion

        private static Task SetCacheAsync<TResult>(ICacheProvider cacheProvider, string key, TResult? value, ICacheableRequirement cacheableRequirement)
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

        private static void SetCache<TResult>(ICacheProvider cacheProvider, string key, TResult? value, ICacheableRequirement cacheableRequirement)
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

        private static string GetCacheKey(ICacheRequirement cacheRequirement, ISpringCachingProxy proxy, ISpringCachingRequirement requirement)
        {
            var keyGenerator = cacheRequirement.KeyGenerator ?? DefaultKeyGenerator.Instance;
            return GetCacheKey(cacheRequirement.Value, keyGenerator.GetKey(cacheRequirement.Key, proxy.Options.KeyExpressionParser, requirement));
        }

        private static string GetCacheKey(string cacheableValue, string? cacheableKey)
        {
            if (string.IsNullOrWhiteSpace(cacheableKey))
            {
                return cacheableValue;
            }
            return $"{cacheableValue}:{cacheableKey}";
        }

        private static bool IsCacheResult<TResult>(ICacheableRequirement cacheableRequirement, ISpringCachingProxy proxy, ISpringCachingRequirement requirement, TResult? result)
        {
            if (cacheableRequirement.UnlessNull && result == null)
            {
                //if result is null, don't cache it
                return false;
            }
            if (string.IsNullOrWhiteSpace(cacheableRequirement.Condition))
            {
                return true;
            }
            var conditionGenerator = cacheableRequirement.ConditionGenerator ?? DefaultPredicateGenerator.Instance;
            return IsPredicate(cacheableRequirement.Condition, conditionGenerator, proxy.Options.ConditionExpressionParser, requirement);
        }


        private static bool IsPredicate(string? expression, IPredicateGenerator predicateGenerator, IBooleanExpressionParser parser, ISpringCachingRequirement requirement)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return true;
            }
            return predicateGenerator.Predicate(expression, parser, requirement);
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
