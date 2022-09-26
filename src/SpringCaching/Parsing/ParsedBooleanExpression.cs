using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Parsing
{
    public readonly struct ParsedBooleanExpression : IParsedBooleanExpression
    {
        public ParsedBooleanExpression(string expression, bool success, bool value)
        {
            Expression = expression;
            Success = success;
            Value = value;
        }
        public string Expression { get; }
        public bool Value { get; }
        public bool Success { get; }
    }
}
