using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public class LocalBuilderDescriptor : EmitDescriptor
#else
    internal class FieldBuilderDescriptor : EmitDescriptor
#endif
    {
        public LocalBuilderDescriptor(LocalBuilder localBuilder, bool canBeNull)
        {
            LocalBuilder = localBuilder;
            CanBeNull = canBeNull;
        }
        public bool CanBeNull { get; }
        public LocalBuilder LocalBuilder { get; }


        public void EmitValue(ILGenerator iLGenerator)
        {
            //TODO : CanBeNull
            iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
        }

    }
}
