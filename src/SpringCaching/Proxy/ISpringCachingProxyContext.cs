using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringCaching.Requirement;

namespace SpringCaching.Proxy
{
    public interface ISpringCachingProxyContext
    {
        ISpringCachingProxy Proxy { get; }
        ISpringCachingRequirement Requirement { get; }

    }
}
