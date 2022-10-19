using SpringCaching.Infrastructure;
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
        private static TResult? InvokeCacheable<TResult>(ISpringCachingProxyContext context, IList<ICacheableRequirement> cacheableRequirements, FuncInvoker<TResult> invoker)
        {
            var proxy = context.Proxy;
            var requirement = context.Requirement;
            KeyValuePair<bool, string?>[] keyDescriptors = new KeyValuePair<bool, string?>[cacheableRequirements.Count];
            int index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                //is read cache?
                bool isCondition = IsConditionPredicate(cacheableRequirement, proxy, requirement);
                if (!isCondition)
                {
                    keyDescriptors[index] = new KeyValuePair<bool, string?>(isCondition, null);
                    continue;
                }
                string key = GetCacheKey(cacheableRequirement, proxy, requirement);
                //cache key
                keyDescriptors[index] = new KeyValuePair<bool, string?>(isCondition, key);
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
                var keyDescriptor = keyDescriptors[index];
                if (!keyDescriptor.Key)
                {
                    continue;
                }
                if (!IsCacheResult(cacheableRequirement, invokerValue))
                {
                    continue;
                }
                //cache it
                SetCache(proxy.CacheProvider, keyDescriptor.Value!, invokerValue, cacheableRequirement);
            }
            return invokerValue;
        }

        private static async Task<TResult?> InvokeCacheableAsync<TResult>(ISpringCachingProxyContext context, IList<ICacheableRequirement> cacheableRequirements, AsyncFuncInvoker<TResult> invoker)
        {
            var proxy = context.Proxy;
            var requirement = context.Requirement;
            KeyValuePair<bool, string?>[] keyDescriptors = new KeyValuePair<bool, string?>[cacheableRequirements.Count];
            int index = -1;
            foreach (var cacheableRequirement in cacheableRequirements)
            {
                index++;
                if (cacheableRequirement == null)
                {
                    continue;
                }
                //is read cache?
                bool isCondition = IsConditionPredicate(cacheableRequirement, proxy, requirement);
                if (!isCondition)
                {
                    keyDescriptors[index] = new KeyValuePair<bool, string?>(isCondition, null);
                    continue;
                }
                string key = GetCacheKey(cacheableRequirement, proxy, requirement);
                //cache key
                keyDescriptors[index] = new KeyValuePair<bool, string?>(isCondition, key);
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
                var keyDescriptor = keyDescriptors[index];
                if (!keyDescriptor.Key)
                {
                    continue;
                }
                if (!IsCacheResult(cacheableRequirement, invokerValue))
                {
                    continue;
                }
                //cache it
                await SetCacheAsync(proxy.CacheProvider, keyDescriptor.Value!, invokerValue, cacheableRequirement).ConfigureAwait(false);
            }
            return invokerValue;
        }

    }
}
