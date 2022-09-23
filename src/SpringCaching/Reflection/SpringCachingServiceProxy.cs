using SpringCaching.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    public static class SpringCachingServiceProxy
    {
        //private static readonly IDictionary<Type, SpringCachingServiceProxyInfo> s_serviceProxyMap = new Dictionary<Type, SpringCachingServiceProxyInfo>();
        private static readonly DynamicAssembly s_dynamicAssembly = new();
        private static readonly string s_suffix = Guid.NewGuid().ToString("N").ToUpper();

#if DEBUG && NET45
        public static DynamicAssembly DynamicAssembly => s_dynamicAssembly;
#endif

        public static TypeInfo? GetProxyType(Type serviceType)
        {
            if (serviceType.IsInterface)
            {
                return null;
            }
            if (serviceType.IsAbstract)
            {
                return null;
            }
            if (serviceType.IsGenericType && serviceType.IsGenericTypeDefinition)
            {
                return null;
            }
            if (serviceType.IsSealed)
            {
                return null;
            }
            //if (s_serviceProxyMap.TryGetValue(serviceType, out var proxyType))
            //{
            //    return proxyType.TypeInfo;
            //}
            var proxyType = new SpringCachingServiceProxyInfo(serviceType)
            {
                DynamicAssembly = s_dynamicAssembly,
                Suffix = s_suffix
            };
            proxyType.Build();
            //s_serviceProxyMap.Add(serviceType, proxyType!);
            return proxyType.TypeInfo;
        }
    }
}
