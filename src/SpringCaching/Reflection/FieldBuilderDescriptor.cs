using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public class FieldBuilderDescriptor : EmitDescriptor
#else
    internal class FieldBuilderDescriptor : EmitDescriptor
#endif
    {
        public FieldBuilderDescriptor(ParameterInfo parameter, FieldBuilder fieldBuilder)
        {
            Parameter = parameter;
            FieldBuilder = fieldBuilder;
        }
        public ParameterInfo Parameter { get; }
        public FieldBuilder FieldBuilder { get; }


        public void EmitValue(ILGenerator iLGenerator, bool box)
        {
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, FieldBuilder);
            //box
            if (box && Parameter.ParameterType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, Parameter.ParameterType);
            }

        }

    }
}
