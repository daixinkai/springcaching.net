using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitEqualNullDescriptor : EmitOperatorDescriptor
    {
        public EmitEqualNullDescriptor(Type type)
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
            iLGenerator.Emit(OpCodes.Ceq);
            var label = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Br_S, label);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.MarkLabel(label);
        }

        private void EmitNullableOperator(ILGenerator iLGenerator)
        {
            // !xx.HasValue
            iLGenerator.EmitNullablePropertyValue(_type.GetProperty("HasValue")!);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.Emit(OpCodes.Ceq);
            //var localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            //iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            ////var label = iLGenerator.DefineLabel();
            ////iLGenerator.Emit(OpCodes.Br_S, label);
            ////iLGenerator.MarkLabel(label);
            //iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
        }

    }
}
