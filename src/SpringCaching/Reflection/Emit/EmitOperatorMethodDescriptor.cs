using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitOperatorMethodDescriptor : EmitOperatorDescriptor
    {
        public EmitOperatorMethodDescriptor(Type type, MethodInfo methodInfo)
        {
            _type = type;
            _methodInfo = methodInfo;
        }

        private readonly Type _type;
        private readonly MethodInfo _methodInfo;

        public override void EmitOperator(ILGenerator iLGenerator)
        {
            if (_methodInfo.DeclaringType != _type)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Callvirt, _methodInfo);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Call, _methodInfo);
            }
        }
    }
}
