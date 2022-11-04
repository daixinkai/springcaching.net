using Microsoft.Extensions.DependencyInjection.Extensions;
using SpringCaching;
using SpringCaching.DependencyInjection;
using SpringCaching.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SpringCachingServiceCollectionExtensions
    {

        public static IDependencyInjectionSpringCachingBuilder AddSpringCaching(this IServiceCollection services)
        {
            return services.AddSpringCaching(options =>
            {
            });
        }

        public static IDependencyInjectionSpringCachingBuilder AddSpringCaching(this IServiceCollection services, Action<SpringCachingOptions> setup)
        {
            var springCachingBuilder = new DependencyInjectionSpringCachingBuilder(services);
            setup?.Invoke(springCachingBuilder.Options);
            services.AddSingleton<ICacheProvider, DistributedCacheProvider>();
            services.AddSingleton(springCachingBuilder.Options);
            //scan service
            foreach (var serviceDescriptor in services.ToList())
            {
                if (serviceDescriptor.ImplementationType != null)
                {
                    var proxyType = CreateProxyType(serviceDescriptor.ImplementationType);
                    if (proxyType != null)
                    {
                        RemoveService(services, serviceDescriptor.ServiceType);
                        services.Add(ServiceDescriptor.Describe(serviceDescriptor.ServiceType, proxyType, serviceDescriptor.Lifetime));
                    }
                }
                else if (serviceDescriptor.ImplementationInstance != null)
                {
                    //not support
                    //var proxyType = CreateProxyType(serviceDescriptor.ImplementationInstance);
                    //if (proxyType != null)
                    //{
                    //    RemoveService(services, serviceDescriptor.ServiceType);
                    //    services.Add(ServiceDescriptor.Singleton(serviceDescriptor.ServiceType, Activator.CreateInstance(proxyType, serviceDescriptor.ImplementationInstance)));
                    //}
                }
                else if (serviceDescriptor.ImplementationFactory != null)
                {
                    //not support
                }
            }
            return springCachingBuilder;
        }


        private static Type? CreateProxyType(Type serviceType)
        {
            if (serviceType.IsDefined(typeof(SpringCachingAttribute), true) && !serviceType.IsDefined(typeof(NonSpringCachingAttribute)))
            {
                var typeInfo = SpringCachingServiceProxy.GetProxyType(serviceType);
                return typeInfo?.AsType();
            }
            return null;
        }

        private static Type? CreateProxyType(object serviceInstance)
        {
            var serviceType = serviceInstance.GetType();
            if (serviceType.IsDefined(typeof(SpringCachingAttribute), true) && !serviceType.IsDefined(typeof(NonSpringCachingAttribute)))
            {
                var typeInfo = SpringCachingServiceProxy.GetProxyType(serviceInstance);
                return typeInfo?.AsType();
            }
            return null;
        }


        private static void RemoveService(IServiceCollection services, Type serviceType)
        {
            services.RemoveAll(serviceType);
        }

    }
}
