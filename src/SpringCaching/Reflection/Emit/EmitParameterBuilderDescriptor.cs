using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitParameterBuilderDescriptor : EmitParameterValueDescriptor
    {
        public EmitParameterBuilderDescriptor(ParameterBuilder parameterBuilder, Type parameterType) : base(parameterBuilder.Name!, parameterType)
        {
            ParameterBuilder = parameterBuilder;
        }
        public ParameterBuilder ParameterBuilder { get; }
        public override void EmitValue(ILGenerator iLGenerator)
        {
            iLGenerator.Emit(OpCodes.Ldarg, ParameterBuilder.Position);
        }

    }
}
