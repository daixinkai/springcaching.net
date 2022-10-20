using SpringCaching.Infrastructure;
using SpringCaching.Parsing;
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
    public static partial class SpringCachingProxyInvoker
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

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
                {
                    InvokeCacheEvict(context, cacheEvictRequirement);
                }
            }

            var cachePutRequirements = context.Requirement.GetCachePutRequirements();

            if (cachePutRequirements != null)
            {
                InvokeCachePut(context, cachePutRequirements, funcInvoker);
            }

            var cacheableRequirements = context.Requirement.GetCacheableRequirements();

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

            if (cacheEvictRequirements != null)
            {
                foreach (var cacheEvictRequirement in cacheEvictRequirements.Where(s => s.BeforeInvocation))
                {
                    await InvokeCacheEvictAsync(context, cacheEvictRequirement).ConfigureAwait(false);
                }
            }

            var cachePutRequirements = context.Requirement.GetCachePutRequirements();

            if (cachePutRequirements != null)
            {
                await InvokeCachePutAsync(context, cachePutRequirements, funcInvoker).ConfigureAwait(false);
            }

            var cacheableRequirements = context.Requirement.GetCacheableRequirements();

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
            if (cacheRequirement.KeyGenerator != null)
            {
                return GetCacheKey(cacheRequirement.Value, cacheRequirement.KeyGenerator.GetKey(cacheRequirement.Key, proxy.Options.KeyExpressionParser, requirement));
            }
            if (cacheRequirement.Key == null)
            {
                return cacheRequirement.Value;
            }
            return GetCacheKey(cacheRequirement.Value, DefaultKeyGenerator.Instance.GetKey(cacheRequirement.Key, proxy.Options.KeyExpressionParser, requirement));
        }

        private static string GetCacheKey(string cacheableValue, string? cacheableKey)
        {
            if (string.IsNullOrWhiteSpace(cacheableKey))
            {
                return cacheableValue + ":null";
            }
            return $"{cacheableValue}:{cacheableKey}";
        }

        private static bool IsCacheResult<TResult>(ICacheableRequirement cacheableRequirement, ISpringCachingProxy proxy, ISpringCachingRequirement requirement, TResult? result)
        {
            return IsCacheResult(cacheableRequirement, result) && IsConditionPredicate(cacheableRequirement, proxy, requirement);
        }

        private static bool IsCacheResult<TResult>(ICacheableRequirement cacheableRequirement, TResult? result)
        {
            if (cacheableRequirement.UnlessNull && result == null)
            {
                //if result is null, don't cache it
                return false;
            }
            return true;
        }

        private static bool IsConditionPredicate(ICacheableRequirement cacheableRequirement, ISpringCachingProxy proxy, ISpringCachingRequirement requirement)
        {
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
