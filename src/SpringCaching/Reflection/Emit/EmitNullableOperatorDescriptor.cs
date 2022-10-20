using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitNullableOperatorDescriptor : EmitOperatorDescriptor
    {

        public EmitNullableOperatorDescriptor(Type type, OperatorType operatorType)
        {
            _type = type;
            _operatorType = operatorType;
        }

        private readonly Type _type;
        private readonly OperatorType _operatorType;

        public override void EmitOperator(ILGenerator iLGenerator)
        {
            throw new NotImplementedException();
        }
    }
}
