using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public abstract class EmitDescriptor
#else
    internal abstract class EmitDescriptor
#endif

    {
    }

}
