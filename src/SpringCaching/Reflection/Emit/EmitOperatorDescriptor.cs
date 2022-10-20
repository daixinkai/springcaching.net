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

        public abstract void EmitOperator(ILGenerator iLGenerator);

        public static EmitOperatorDescriptor? TryCreate(Type leftType, OperatorType operatorType, ExpressionTokenDescriptor right)
        {

            if (right!.Token.TokenType == ExpressionTokenType.Value && right!.Token.Value == "null")
            {
                return operatorType == OperatorType.NotEqual ?
                    new EmitNotEqualNullDescriptor(leftType) :
                    new EmitEqualNullDescriptor(leftType);
            }

            if (leftType.IsNullableType())
            {
                return TryCreateNullable(leftType, operatorType);
            }

            string? methodName = operatorType switch
            {
                OperatorType.Equal => "op_Equality",
                OperatorType.NotEqual => "op_Inequality",
                _ => null
            };
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }
            var method = leftType.GetMethod(methodName);
            if (method == null)
            {
                return null;
            }
            return new EmitOperatorMethodDescriptor(leftType, method);
        }

        private static EmitOperatorDescriptor? TryCreateNullable(Type type, OperatorType operatorType)
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
