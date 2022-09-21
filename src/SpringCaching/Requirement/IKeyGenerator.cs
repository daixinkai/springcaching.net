using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public interface IKeyGenerator
    {
        string? GetKey(string? key, ISpringCachingRequirement requirement);
    }
}
