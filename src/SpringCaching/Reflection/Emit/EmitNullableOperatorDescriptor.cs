using SpringCaching.Internal;
using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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

        private LocalBuilder? _localBuilder;


        //public override void PreEmitOperator(ILGenerator iLGenerator)
        //{
        //    var method = _type.GetMethod("GetValueOrDefault", Type.EmptyTypes);
        //    _localBuilder = iLGenerator.EmitNullableMethod(method!);
        //}

        //public override void PostEmitOperator(ILGenerator iLGenerator)
        //{
        //    ExpressionTokenHelper.EmitOperatorType(iLGenerator, _operatorType);
        //    iLGenerator.EmitNullablePropertyValue(_type.GetProperty("HasValue")!, ref _localBuilder);
        //    iLGenerator.Emit(OpCodes.And);

        //}

        public override void PreEmitOperator(ILGenerator iLGenerator)
        {
            _localBuilder = iLGenerator.EmitNullablePropertyValue(_type.GetProperty("HasValue")!);
            iLGenerator.EmitNullablePropertyValue(_type.GetProperty("Value")!, ref _localBuilder);
        }

        public override void PostEmitOperator(ILGenerator iLGenerator)
        {
            ExpressionTokenHelper.EmitOperatorType(iLGenerator, _operatorType);
            iLGenerator.Emit(OpCodes.And);

        }

    }
}
