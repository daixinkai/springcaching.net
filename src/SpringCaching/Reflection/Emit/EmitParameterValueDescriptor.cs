using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal abstract class EmitParameterValueDescriptor : EmitValueDescriptor
    {
        public EmitParameterValueDescriptor(string parameterName, Type parameterType)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
        }

        public string ParameterName { get; }

        public Type ParameterType { get; }

        public sealed override Type EmitValueType => ParameterType;

    }
}
