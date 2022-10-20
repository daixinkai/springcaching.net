using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitLocalBuilderDescriptor : EmitValueDescriptor
    {
        public EmitLocalBuilderDescriptor(LocalBuilder localBuilder)
        {
            LocalBuilder = localBuilder;
        }
        public LocalBuilder LocalBuilder { get; }

        public override Type EmitValueType => LocalBuilder.LocalType;

        public override void EmitValue(ILGenerator iLGenerator)
        {
            iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
        }

    }
}
