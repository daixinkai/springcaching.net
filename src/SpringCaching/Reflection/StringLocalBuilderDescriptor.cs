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
    internal class StringLocalBuilderDescriptor : EmitValueDescriptor
#endif
    {
        public StringLocalBuilderDescriptor(LocalBuilder localBuilder, string? defaultValue)
        {
            LocalBuilder = localBuilder;
            DefaultValue = defaultValue;
        }
        public string? DefaultValue { get; }
        public LocalBuilder LocalBuilder { get; }

        public override Type EmitValueType => LocalBuilder.LocalType;

        public override void EmitValue(ILGenerator iLGenerator, bool box)
        {
            if (DefaultValue == null)
            {
                iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
                return;
            }

            // if value is null , return defaultValue
            var localBuilder = iLGenerator.DeclareLocal(typeof(string));
            Label trueLabel = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
            iLGenerator.Emit(OpCodes.Dup);
            iLGenerator.Emit(OpCodes.Brtrue_S, trueLabel);
            iLGenerator.Emit(OpCodes.Pop);
            iLGenerator.Emit(OpCodes.Ldstr, DefaultValue);

            iLGenerator.MarkLabel(trueLabel);
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            iLGenerator.Emit(OpCodes.Ldloc, localBuilder);

        }

    }
}
