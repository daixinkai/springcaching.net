using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Proxy
{
    public interface ISpringCachingProxy
    {
        ICacheProvider CacheProvider { get; }
        SpringCachingOptions Options { get; }

    }
}
