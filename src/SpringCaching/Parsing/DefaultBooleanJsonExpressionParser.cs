using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Parsing
{
    internal class DefaultBooleanJsonExpressionParser : DefaultJsonExpressionParser, IBooleanExpressionParser
    {
        public IParsedBooleanExpression Parse(string expression, IDictionary<string, object>? arguments)
        {
            return new ParsedBooleanExpression(expression, true, true);
        }
    }
}
