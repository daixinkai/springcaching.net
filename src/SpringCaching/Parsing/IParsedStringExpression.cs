using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Parsing
{
    public interface IParsedStringExpression : IParsedExpression
    {
        string? Value { get; }
    }
}
