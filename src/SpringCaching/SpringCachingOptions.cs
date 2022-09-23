using SpringCaching.Infrastructure.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching
{
    public class SpringCachingOptions
    {
        public IStringExpressionParser KeyExpressionParser { get; set; } = new DefaultStringJsonExpressionParser();
        public IBooleanExpressionParser ConditionExpressionParser { get; set; } = new DefaultBooleanJsonExpressionParser();
    }
}
