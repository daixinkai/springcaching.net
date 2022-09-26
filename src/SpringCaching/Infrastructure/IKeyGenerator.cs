using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringCaching.Parsing;
using SpringCaching.Requirement;

namespace SpringCaching.Infrastructure
{
    public interface IKeyGenerator
    {
        string? GetKey(string? expression, IStringExpressionParser parser, ISpringCachingRequirement requirement);
    }
}
