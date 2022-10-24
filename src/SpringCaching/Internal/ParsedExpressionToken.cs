using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Internal
{
    internal class ParsedExpressionToken
    {
        public ParsedExpressionToken(ExpressionToken token)
        {
            Token = token;
        }
        public ExpressionToken Token { get; }
        public ExpressionToken? NextToken { get; set; }
    }
}
