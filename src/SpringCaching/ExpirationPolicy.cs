using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching
{
    public enum ExpirationPolicy
    {
        None = 0,
        Absolute = 1,
        Sliding = 2
    }
}
