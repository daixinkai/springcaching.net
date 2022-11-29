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

        private static readonly DynamicAssembly s_dynamicAssembly = new();


#if DEBUG && NET45
        private static readonly string s_suffix = "BB93BE8338034C5FBC946FB266372676";
        public static DynamicAssembly DynamicAssembly => s_dynamicAssembly;
#else   
        private static readonly string s_suffix = Guid.NewGuid().ToString("N").ToUpper();
#endif

        public static SpringCachingServiceProxyInfo? GetServiceProxyInfo(Type serviceType)
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
            var proxyType = new SpringCachingServiceProxyInfo(serviceType, s_dynamicAssembly)
            {
                Suffix = s_suffix
            };
            proxyType.Build();
            return proxyType;
        }

        //public static TypeInfo? GetProxyType(object serviceInstance)
        //{
        //    if (serviceInstance == null)
        //    {
        //        return null;
        //    }
        //    var serviceType = serviceInstance.GetType();
        //    if (serviceType.IsGenericType && serviceType.IsGenericTypeDefinition)
        //    {
        //        return null;
        //    }
        //    if (serviceType.IsSealed)
        //    {
        //        return null;
        //    }
        //    //if (s_serviceProxyMap.TryGetValue(serviceType, out var proxyType))
        //    //{
        //    //    return proxyType.TypeInfo;
        //    //}
        //    var proxyType = new SpringCachingServiceInstanceProxyInfo(serviceType)
        //    {
        //        DynamicAssembly = s_dynamicAssembly,
        //        Suffix = s_suffix
        //    };
        //    proxyType.Build();
        //    //s_serviceProxyMap.Add(serviceType, proxyType!);
        //    return proxyType.TypeInfo;
        //}

    }
}
