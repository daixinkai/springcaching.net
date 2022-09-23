using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure.Parser
{
    internal class DefaultBooleanJsonExpressionParser : DefaultJsonExpressionParser, IBooleanExpressionParser
    {
        public IParsedBooleanExpression Parse(string expression, IDictionary<string, object>? arguments)
        {
            return new ParsedBooleanExpression(expression, false, false);
        }
    }
}
