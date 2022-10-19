using SpringCaching.Internal;
using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal class BooleanExpressionTokenDescriptor
    {
        //private static readonly OperatorType[] s_supportOperatorTypes = new[] {
        //                OperatorType.LogicalAnd,
        //                OperatorType.LogicalOr,
        //                OperatorType.Equal,
        //                OperatorType.NotEqual
        //            };

        public BooleanExpressionTokenDescriptor(ExpressionTokenDescriptor left)
        {
            Left = left;
        }

        public enum ExpressionType
        {
            Value,
            Compare
        }

        public ExpressionTokenDescriptor Left { get; }

        public ExpressionType Type => Compare == null ? ExpressionType.Value : ExpressionType.Compare;

        public ExpressionToken? Compare { get; set; }
        public ExpressionTokenDescriptor? Right { get; set; }


        public bool IsCompleted => Compare == null && Right == null || Compare != null && Right != null;

        public static List<BooleanExpressionTokenDescriptor> FromTokens(IList<ExpressionToken> tokens, IList<FieldBuilderDescriptor>? fieldBuilderDescriptors)
        {
            List<BooleanExpressionTokenDescriptor> descriptors = new List<BooleanExpressionTokenDescriptor>();

            BooleanExpressionTokenDescriptor? currentDescriptor = null;

            foreach (var token in tokens)
            {
                if (token.TokenType == ExpressionTokenType.Field
                    || token.TokenType == ExpressionTokenType.Value
                    || token.TokenType == ExpressionTokenType.SingleQuoted
                    || token.TokenType == ExpressionTokenType.DoubleQuoted
                    )
                {
                    if (currentDescriptor == null || currentDescriptor.Compare == null)
                    {
                        currentDescriptor = new BooleanExpressionTokenDescriptor(new ExpressionTokenDescriptor(token, fieldBuilderDescriptors));
                    }
                    else
                    {
                        currentDescriptor.Right = new ExpressionTokenDescriptor(token, fieldBuilderDescriptors);
                        descriptors.Add(currentDescriptor);
                        currentDescriptor = null;
                    }
                    continue;
                }
                else if (token.TokenType == ExpressionTokenType.Operator)
                {
                    //if (currentDescriptor != null && s_supportOperatorTypes.Contains(token.OperatorType))
                    if ((token.OperatorType == OperatorType.LogicalAnd || token.OperatorType == OperatorType.LogicalOr) && currentDescriptor != null)
                    {
                        descriptors.Add(currentDescriptor);
                        currentDescriptor = null;
                    }
                    if (currentDescriptor != null && currentDescriptor.Compare == null)
                    {
                        currentDescriptor.Compare = token;
                    }
                }
            }
            if (currentDescriptor != null && currentDescriptor.IsCompleted)
            {
                descriptors.Add(currentDescriptor);
            }
            return descriptors;
        }


        public LocalBuilderDescriptor? EmitValue(ILGenerator iLGenerator, IList<FieldBuilderDescriptor> descriptors)
        {
            bool isCompare = Type == ExpressionType.Compare;
            var leftType = EmitExpressionToken(iLGenerator, Left, descriptors, !isCompare, out var leftLocalBuilder);
            if (leftType == null)
            {
                if (!isCompare)
                {
                    EmitConstantExpressionToken(iLGenerator, "true", typeof(bool), true, out leftLocalBuilder);
                }
                return leftLocalBuilder;
            }
            if (!isCompare)
            {
                return leftLocalBuilder;
            }

            var compareMethod = GetCompareMethod(leftType, Compare!.OperatorType);

            bool compareNull = Right!.Token.TokenType == ExpressionTokenType.Value && Right!.Token.Value == "null";
            if (compareNull)
            {
                compareMethod = null;
                iLGenerator.Emit(OpCodes.Ldnull);
                if (Compare!.OperatorType == OperatorType.NotEqual)
                {
                    iLGenerator.Emit(OpCodes.Cgt_Un);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ceq);
                }
            }
            else
            {
                if (compareMethod == null)
                {
                    ExpressionTokenHelper.EmitOperatorType(iLGenerator, Compare!.OperatorType);
                }
                var rightType = EmitExpressionToken(iLGenerator, Right!, descriptors, false, out var rightLocalBuilder);
                if (rightType == null)
                {
                    EmitConstantExpressionToken(iLGenerator, "true", typeof(bool), false, out rightLocalBuilder);
                }
            }


            if (compareMethod != null)
            {
                iLGenerator.Emit(OpCodes.Call, compareMethod);
            }
            else if (!compareNull)
            {
                var label = iLGenerator.DefineLabel();
                iLGenerator.Emit(OpCodes.Br_S, label);
                iLGenerator.Emit(OpCodes.Ldc_I4_0);
                iLGenerator.MarkLabel(label);
            }


            //leftLocalBuilder.EmitValue(iLGenerator, false);
            //rightLocalBuilder!.EmitValue(iLGenerator, false);
            LocalBuilder? localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            //iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
            return new LocalBuilderDescriptor(localBuilder);
        }

        private MethodInfo? GetCompareMethod(Type type, OperatorType operatorType)
        {
            string? methodName = operatorType switch
            {
                OperatorType.Equal => "op_Equality",
                OperatorType.NotEqual => "op_Inequality",
                _ => null
            };
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }
            return type.GetMethod(methodName);
        }



        #region private

        private static Type? EmitExpressionToken(ILGenerator iLGenerator, ExpressionTokenDescriptor tokenDescriptor, IList<FieldBuilderDescriptor> descriptors, bool declareLocal, out LocalBuilderDescriptor? descriptor)
        {
            descriptor = null;
            switch (tokenDescriptor.TokenType)
            {
                case ExpressionTokenType.Operator:
                    break;
                case ExpressionTokenType.Function:
                    break;
                case ExpressionTokenType.Comma:
                    break;
                case ExpressionTokenType.Field:
                    return EmitFieldExpressionToken(iLGenerator, tokenDescriptor, descriptors, declareLocal, out descriptor);
                case ExpressionTokenType.SingleQuoted:
                case ExpressionTokenType.DoubleQuoted:
                    return EmitConstantExpressionToken(iLGenerator, tokenDescriptor.Token.Value!, typeof(string), declareLocal, out descriptor);
                case ExpressionTokenType.Value:
                    return EmitConstantExpressionToken(iLGenerator, tokenDescriptor.Token.Value!, typeof(int), declareLocal, out descriptor);
                default:
                    return null;
            }
            return null;
        }

        private static Type? EmitFieldExpressionToken(
            ILGenerator iLGenerator,
            ExpressionTokenDescriptor tokenDescriptor,
            IList<FieldBuilderDescriptor> descriptors,
            bool declareLocal,
            out LocalBuilderDescriptor? descriptor
            )
        {
            descriptor = null;

            var callPropertyDescriptor = tokenDescriptor.CallPropertyDescriptor ?? ExpressionTokenHelper.GetCallPropertyDescriptor(tokenDescriptor.Token, descriptors);
            if (callPropertyDescriptor == null)
            {
                return null;
            }

            var lastDescriptor = callPropertyDescriptor.EmitValue(iLGenerator);
            if (declareLocal)
            {
                var localBuilder = iLGenerator.DeclareLocal(callPropertyDescriptor.EmitValueType);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                descriptor = new LocalBuilderDescriptor(localBuilder);
            }
            return lastDescriptor.EmitValueType;
        }


        private static Type? EmitConstantExpressionToken(ILGenerator iLGenerator, string value, Type type, bool declareLocal, out LocalBuilderDescriptor? descriptor)
        {
            descriptor = null;
            if (type == typeof(string))
            {
                iLGenerator.Emit(OpCodes.Ldstr, value.ToString());
            }
            else if (type == typeof(bool) && bool.TryParse(value, out var boolValue))
            {
                if (boolValue)
                {
                    iLGenerator.Emit(OpCodes.Ldc_I4_1);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                }
            }
            else if (type == typeof(int) && int.TryParse(value, out var intValue))
            {
                iLGenerator.Emit(OpCodes.Ldc_I4, intValue);
            }
            else if (type == typeof(long) && long.TryParse(value, out var longValue))
            {
                iLGenerator.Emit(OpCodes.Ldc_I8, longValue);
            }
            else
            {
                return null;
            }
            if (declareLocal)
            {
                var localBuilder = iLGenerator.DeclareLocal(type);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                descriptor = new LocalBuilderDescriptor(localBuilder);
            }
            return type;
        }


        #endregion

    }
}
