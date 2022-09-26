using SpringCaching.Parsing;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    internal class DefaultKeyGenerator : IKeyGenerator
    {
        public static readonly DefaultKeyGenerator Instance = new DefaultKeyGenerator();
        public string? GetKey(string? expression, IStringExpressionParser parser, ISpringCachingRequirement requirement)
        {
            if (expression == null)
            {
                return null;
            }
            return parser.Parse(expression, requirement.Arguments).Value;
        }
    }
}
