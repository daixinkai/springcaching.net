using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure.Parser
{
    public interface IParsedExpression
    {
        string Expression { get; }
        bool Success { get; }
    }
}
