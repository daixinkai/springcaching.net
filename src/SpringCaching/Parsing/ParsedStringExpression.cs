using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Parsing
{
    public readonly struct ParsedStringExpression : IParsedStringExpression
    {
        public ParsedStringExpression(string expression, bool success, string? value)
        {
            Expression = expression;
            Success = success;
            Value = value;
        }
        public string Expression { get; }
        public bool Success { get; }
        public string? Value { get; }
    }
}
