using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitFieldBuilderDescriptor : EmitParameterValueDescriptor
    {
        public EmitFieldBuilderDescriptor(ParameterInfo parameter, FieldBuilder fieldBuilder) : base(parameter.Name!, parameter.ParameterType)
        {
            FieldBuilder = fieldBuilder;
        }
        public FieldBuilder FieldBuilder { get; }

        public override void EmitValue(ILGenerator iLGenerator)
        {
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, FieldBuilder);
        }

    }
}
