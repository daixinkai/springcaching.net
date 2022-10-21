using SpringCaching.Internal;
using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal abstract class EmitOperatorDescriptor
    {

        public class DefaultOperatorDescriptor : EmitOperatorDescriptor
        {
            public DefaultOperatorDescriptor(OperatorType operatorType)
            {
                _operatorType = operatorType;
            }

            private readonly OperatorType _operatorType;

            public override void PreEmitOperator(ILGenerator iLGenerator)
            {

            }

            public override void PostEmitOperator(ILGenerator iLGenerator)
            {
                ExpressionTokenHelper.EmitOperatorType(iLGenerator, _operatorType);
            }

        }

        public abstract void PreEmitOperator(ILGenerator iLGenerator);

        public abstract void PostEmitOperator(ILGenerator iLGenerator);

        public static EmitOperatorDescriptor Create(Type leftType, OperatorType operatorType, ExpressionTokenDescriptor right)
        {

            if (right!.Token.TokenType == ExpressionTokenType.Value && right!.Token.Value == "null")
            {
                return operatorType == OperatorType.NotEqual ?
                    new EmitNotEqualNullDescriptor(leftType) :
                    new EmitEqualNullDescriptor(leftType);
            }


            if (leftType.IsNullableType())
            {
                var nullableOperatorDescriptor = TryCreateNullable(leftType, operatorType);
                if (nullableOperatorDescriptor != null)
                {
                    return nullableOperatorDescriptor;
                }
            }

            string? methodName = operatorType switch
            {
                OperatorType.Equal => "op_Equality",
                OperatorType.NotEqual => "op_Inequality",
                _ => null
            };

            var method = string.IsNullOrWhiteSpace(methodName) ? null : leftType.GetMethod(methodName);
            if (method == null)
            {
                return new DefaultOperatorDescriptor(operatorType);
            }
            return new EmitOperatorMethodDescriptor(leftType, method);
        }

        private static EmitNullableOperatorDescriptor? TryCreateNullable(Type type, OperatorType operatorType)
        {
            if (operatorType == OperatorType.Equal ||
                operatorType == OperatorType.NotEqual ||
                operatorType == OperatorType.GreaterThan ||
                operatorType == OperatorType.LessThan ||
                operatorType == OperatorType.GreaterThanOrEqual ||
                operatorType == OperatorType.LessThanOrEqual)
            {
                return new EmitNullableOperatorDescriptor(type, operatorType);
            }
            return null;
        }

    }
}
