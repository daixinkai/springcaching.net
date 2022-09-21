using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching.DependencyInjection
{
    public interface IDependencyInjectionSpringCachingBuilder : ISpringCachingBuilder
    {
        IServiceCollection Services { get; }
    }
}
