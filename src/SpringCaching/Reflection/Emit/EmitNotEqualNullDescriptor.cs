using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitNotEqualNullDescriptor : EmitOperatorDescriptor
    {
        public EmitNotEqualNullDescriptor(Type type)
        {
            _type = type;
        }

        private Type _type;

        public override void EmitOperator(ILGenerator iLGenerator)
        {
            if (_type.IsNullableType())
            {
                EmitNullableOperator(iLGenerator);
                return;
            }
            iLGenerator.Emit(OpCodes.Ldnull);
            iLGenerator.Emit(OpCodes.Cgt_Un);
            var label = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Br_S, label);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.MarkLabel(label);
        }

        private void EmitNullableOperator(ILGenerator iLGenerator)
        {
            // xx.HasValue
            iLGenerator.EmitNullablePropertyValue(_type.GetProperty("HasValue")!);
        }
    }
}
