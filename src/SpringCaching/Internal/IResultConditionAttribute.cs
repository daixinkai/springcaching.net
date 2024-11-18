using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Internal
{
    internal interface IResultConditionAttribute
    {
        /// <summary>
        /// Spring Expression Language (SpEL) expression used for making the method caching conditional.
        /// Default is "", meaning the method result is always cached.
        /// start with #result
        /// </summary>
        string? ResultCondition { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// The bean name of the custom <see cref="IResultPredicateGenerator"/> or <see cref="IResultPredicateGenerator{TResult}"/> to use.
        /// Mutually exclusive with the key attribute.
        /// </summary>
        Type? ResultConditionGenerator { get; set; }
    }
}
