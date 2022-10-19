using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal class FieldBuilderDescriptor : EmitValueDescriptor
    {
        public FieldBuilderDescriptor(ParameterInfo parameter, FieldBuilder fieldBuilder)
        {
            Parameter = parameter;
            FieldBuilder = fieldBuilder;
        }
        public ParameterInfo Parameter { get; }
        public FieldBuilder FieldBuilder { get; }

        public override Type EmitValueType => Parameter.ParameterType;

        public override void EmitValue(ILGenerator iLGenerator)
        {
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, FieldBuilder);
        }

    }
}
