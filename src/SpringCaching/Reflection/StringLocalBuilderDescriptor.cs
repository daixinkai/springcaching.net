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
    public class StringLocalBuilderDescriptor : EmitValueDescriptor
#else
    internal class FieldBuilderDescriptor : EmitDescriptor
#endif
    {
        public StringLocalBuilderDescriptor(LocalBuilder localBuilder, bool canBeNull)
        {
            LocalBuilder = localBuilder;
            CanBeNull = canBeNull;
        }
        public bool CanBeNull { get; }
        public LocalBuilder LocalBuilder { get; }

        public override Type EmitValueType => LocalBuilder.LocalType;

        public override void EmitValue(ILGenerator iLGenerator, bool box)
        {
            if (!CanBeNull)
            {
                iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
                return;
            }

            // if value is null , return "null"
            var localBuilder = iLGenerator.DeclareLocal(typeof(string));
            Label trueLabel = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
            iLGenerator.Emit(OpCodes.Dup);
            iLGenerator.Emit(OpCodes.Brtrue_S, trueLabel);
            iLGenerator.Emit(OpCodes.Pop);
            iLGenerator.Emit(OpCodes.Ldstr, "null");

            iLGenerator.MarkLabel(trueLabel);
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            iLGenerator.Emit(OpCodes.Ldloc, localBuilder);

        }

    }
}
