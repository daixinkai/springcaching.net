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
        private static void InvokeCacheEvict(ISpringCachingProxyContext context, ICacheEvictRequirement cacheEvictRequirement)
        {
            if (cacheEvictRequirement == null)
            {
                return;
            }
            if (cacheEvictRequirement.AllEntries)
            {
                string key = string.IsNullOrWhiteSpace(cacheEvictRequirement.Key) ?
                    cacheEvictRequirement.Value :
                    GetCacheKey(cacheEvictRequirement, context.Proxy, context.Requirement);
                context.Proxy.CacheProvider.Clear(key);
            }
            else
            {
                string key = GetCacheKey(cacheEvictRequirement, context.Proxy, context.Requirement);
                context.Proxy.CacheProvider.Remove(key);
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
                string key = string.IsNullOrWhiteSpace(cacheEvictRequirement.Key) ?
                    cacheEvictRequirement.Value :
                    GetCacheKey(cacheEvictRequirement, context.Proxy, context.Requirement);
                return context.Proxy.CacheProvider.ClearAsync(key);
            }
            else
            {
                string key = GetCacheKey(cacheEvictRequirement, context.Proxy, context.Requirement);
                return context.Proxy.CacheProvider.RemoveAsync(key);
            }
        }

    }
}
