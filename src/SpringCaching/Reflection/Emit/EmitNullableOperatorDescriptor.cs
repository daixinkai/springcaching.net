using SpringCaching.Internal;
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

        public override void PreEmitOperator(ILGenerator iLGenerator)
        {
            var method = _type.GetMethod("GetValueOrDefault", Type.EmptyTypes);
            //iLGenerator.Emit(OpCodes.Call, method!);
            iLGenerator.EmitNullableMethod(method!);
        }

        public override void PostEmitOperator(ILGenerator iLGenerator)
        {

            ExpressionTokenHelper.EmitOperatorType(iLGenerator, _operatorType);
            //switch (_operatorType)
            //{
            //    case OperatorType.Equal:
            //        break;
            //    case OperatorType.NotEqual:
            //        break;
            //    case OperatorType.LessThan:
            //        break;
            //    case OperatorType.LessThanOrEqual:
            //        break;
            //    case OperatorType.GreaterThan:
            //        break;
            //    case OperatorType.GreaterThanOrEqual:
            //        break;
            //    default:
            //        break;
            //}

            //throw new NotImplementedException();
        }

    }
}
