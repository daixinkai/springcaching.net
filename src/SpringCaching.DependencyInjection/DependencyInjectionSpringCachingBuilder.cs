using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace SpringCaching.DependencyInjection
{
    internal class DependencyInjectionSpringCachingBuilder : IDependencyInjectionSpringCachingBuilder
    {
        public DependencyInjectionSpringCachingBuilder(IServiceCollection services)
        {
            Services = services;
            Options = new SpringCachingOptions();
        }
        public IServiceCollection Services { get; }
        public SpringCachingOptions Options { get; set; }
    }
}
