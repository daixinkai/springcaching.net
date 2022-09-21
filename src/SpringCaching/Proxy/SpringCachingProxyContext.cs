using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpringCaching.Internal;
using SpringCaching.Requirement;

namespace SpringCaching.Proxy
{
    public class SpringCachingProxyContext : ISpringCachingProxyContext
    {
        public SpringCachingProxyContext(ISpringCachingProxy proxy, ISpringCachingRequirement requirement)
        {
            Proxy = proxy;
            Requirement = requirement;
        }
        public ISpringCachingProxy Proxy { get; }
        public ISpringCachingRequirement Requirement { get; }


    }
}
