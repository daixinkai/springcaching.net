using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Parsing
{
    public interface IExpressionParser<T> where T : IParsedExpression
    {
        T Parse(string expression, IDictionary<string, object>? arguments);
    }
}
