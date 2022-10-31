using SpringCaching.Parsing;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public class FuncKeyGenerator : IKeyGenerator
    {

        public FuncKeyGenerator(Func<string> func)
        {
            _func = func;
        }

        private readonly Func<string> _func;

        string? IKeyGenerator.GetKey(string? expression, ISpringCachingRequirement requirement)
        {
            return _func.Invoke();
        }

    }
}
