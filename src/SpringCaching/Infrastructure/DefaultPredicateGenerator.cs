using SpringCaching.Infrastructure.Parser;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    internal class DefaultPredicateGenerator : IPredicateGenerator
    {
        public static readonly DefaultPredicateGenerator Instance = new DefaultPredicateGenerator();
        public bool Predicate(string? expression, IBooleanExpressionParser parser, ISpringCachingRequirement requirement)
        {
            if (expression == null)
            {
                return true;
            }
            return parser.Parse(expression, requirement.Arguments).Value;
        }
    }
}
