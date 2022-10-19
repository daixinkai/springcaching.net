using SpringCaching.Infrastructure;
using SpringCaching.Internal;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Proxy
{
    partial class SpringCachingProxyInvoker
    {
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

    }
}
