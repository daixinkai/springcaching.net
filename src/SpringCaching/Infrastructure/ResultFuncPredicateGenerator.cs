using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public class ResultFuncPredicateGenerator : IResultPredicateGenerator
    {
        public ResultFuncPredicateGenerator(Func<object?, bool> func)
        {
            _func = func;
        }

        private readonly Func<object?, bool> _func;

        bool IResultPredicateGenerator.Predicate(string? expression, ISpringCachingRequirement requirement, object? result)
        {
            return _func.Invoke(result);
        }

    }

    public class ResultFuncPredicateGenerator<TResult> : IResultPredicateGenerator<TResult>
    {
        public ResultFuncPredicateGenerator(Func<TResult?, bool> func)
        {
            _func = func;
        }

        private readonly Func<TResult?, bool> _func;

        bool IResultPredicateGenerator<TResult>.Predicate(string? expression, ISpringCachingRequirement requirement, TResult? result)
        {
            return _func.Invoke(result);
        }

        bool IResultPredicateGenerator.Predicate(string? expression, ISpringCachingRequirement requirement, object? result)
        {
            return _func.Invoke((TResult?)result);
        }
    }
}
