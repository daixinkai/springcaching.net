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
    public class LocalBuilderDescriptor : EmitValueDescriptor
#else
    internal class LocalBuilderDescriptor : EmitDescriptor
#endif
    {
        public LocalBuilderDescriptor(LocalBuilder localBuilder)
        {
            LocalBuilder = localBuilder;
        }
        public LocalBuilder LocalBuilder { get; }

        public override Type EmitValueType => LocalBuilder.LocalType;

        public override void EmitValue(ILGenerator iLGenerator, bool box)
        {
            iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
            EmitBox(iLGenerator, box);
        }

    }
}
