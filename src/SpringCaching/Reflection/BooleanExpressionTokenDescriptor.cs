using SpringCaching.Internal;
using SpringCaching.Parsing;
using SpringCaching.Reflection.Emit;
using System;
using System.CodeDom;
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


        public BooleanExpressionTokenDescriptor()
        {
        }

        public BooleanExpressionTokenDescriptor(ExpressionTokenDescriptor left)
        {
            Left = left;
        }

        public enum ExpressionType
        {
            Value,
            Compare
        }

        public ExpressionToken? OpenParenthesisToken { get; private set; }
        public ExpressionToken? CloseParenthesisToken { get; private set; }

        public ExpressionToken? ConnectOperatorToken { get; private set; }

        public ExpressionTokenDescriptor? Left { get; private set; }

        public ExpressionType Type
        {
            get
            {
                if (Compare == null)
                {
                    return ExpressionType.Value;
                }
                //if (Compare.OperatorType == OperatorType.LogicalNegation)
                //{
                //    return ExpressionType.LogicalNegation;
                //}
                return ExpressionType.Compare;
            }
        }

        public ExpressionToken? Compare { get; set; }
        public ExpressionTokenDescriptor? Right { get; set; }

        public bool IsLogicalNegation { get; set; }

        public string Debug
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                //if (OpenParenthesisToken != null)
                //{
                //    stringBuilder.Append("(");
                //}

                if (IsLogicalNegation)
                {
                    stringBuilder.Append("!");
                }

                stringBuilder.Append(Left!.Token.Value!);

                if (Compare != null)
                {
                    stringBuilder.Append(" " + Compare!.Value!);
                }

                if (Right != null)
                {
                    stringBuilder.Append(" " + Right!.Token.Value!);
                }

                //if (CloseParenthesisToken != null)
                //{
                //    stringBuilder.Append(")");
                //}
                return stringBuilder.ToString();
            }
        }

        public bool IsCompleted
        {
            get
            {
                //Left != null && (Compare == null && Right == null || Compare != null && Right != null);
                return Type switch
                {
                    ExpressionType.Value => Left != null && Right == null,
                    ExpressionType.Compare => Left != null && Right != null,
                    //ExpressionType.LogicalNegation => Left != null && Right == null,
                    _ => false
                };
            }
        }

        public static List<BooleanExpressionTokenDescriptor> FromTokens(IList<ExpressionToken> tokens, IList<EmitFieldBuilderDescriptor>? fieldBuilderDescriptors)
        {
            List<BooleanExpressionTokenDescriptor> descriptors = new List<BooleanExpressionTokenDescriptor>();

            BooleanExpressionTokenDescriptor currentDescriptor = new BooleanExpressionTokenDescriptor();

            foreach (var token in tokens)
            {
                if (token.TokenType == ExpressionTokenType.OpenParenthesis)
                {
                    if (currentDescriptor!.IsCompleted)
                    {
                        descriptors.Add(currentDescriptor);
                    }
                    currentDescriptor = new BooleanExpressionTokenDescriptor();
                    currentDescriptor.OpenParenthesisToken = token;
                }
                else if (token.TokenType == ExpressionTokenType.CloseParenthesis)
                {
                    if (currentDescriptor!.IsCompleted)
                    {
                        descriptors.Add(currentDescriptor);
                        currentDescriptor.CloseParenthesisToken = token;
                        currentDescriptor = new BooleanExpressionTokenDescriptor();
                    }
                }
                else if (token.TokenType == ExpressionTokenType.Field
                    || token.TokenType == ExpressionTokenType.Value
                    || token.TokenType == ExpressionTokenType.SingleQuoted
                    || token.TokenType == ExpressionTokenType.DoubleQuoted
                    )
                {
                    if (currentDescriptor!.Left == null)
                    {
                        currentDescriptor.Left = new ExpressionTokenDescriptor(token, fieldBuilderDescriptors);
                    }
                    else if (currentDescriptor!.Compare == null)
                    {
                        currentDescriptor = new BooleanExpressionTokenDescriptor();
                        currentDescriptor.Left = new ExpressionTokenDescriptor(token, fieldBuilderDescriptors);
                    }
                    else if (currentDescriptor.Right == null)
                    {
                        currentDescriptor.Right = new ExpressionTokenDescriptor(token, fieldBuilderDescriptors);
                    }
                    else
                    {
                        currentDescriptor = new BooleanExpressionTokenDescriptor();
                    }
                }
                else if (token.TokenType == ExpressionTokenType.Operator)
                {
                    //if (currentDescriptor != null && s_supportOperatorTypes.Contains(token.OperatorType))
                    if (
                        token.OperatorType == OperatorType.LogicalAnd ||
                        token.OperatorType == OperatorType.LogicalOr
                        )
                    {
                        if (currentDescriptor.IsCompleted)
                        {
                            descriptors.Add(currentDescriptor);
                        }
                        currentDescriptor = new BooleanExpressionTokenDescriptor();
                        currentDescriptor!.ConnectOperatorToken = token;
                    }
                    else if (token.OperatorType == OperatorType.LogicalNegation)
                    {
                        currentDescriptor.IsLogicalNegation = true;
                        //if (currentDescriptor.IsCompleted)
                        //{
                        //    descriptors.Add(currentDescriptor);
                        //}
                        //currentDescriptor = new BooleanExpressionTokenDescriptor();
                    }
                    if (currentDescriptor!.Left != null && currentDescriptor.Compare == null)
                    {
                        currentDescriptor.Compare = token;
                    }
                }
            }
            if (currentDescriptor.IsCompleted)
            {
                descriptors.Add(currentDescriptor);
            }
            return descriptors;
        }

        public static List<BooleanExpressionTokenDescriptor> FromTokensOld(IList<ExpressionToken> tokens, IList<EmitFieldBuilderDescriptor>? fieldBuilderDescriptors)
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

        public EmitLocalBuilderDescriptor? EmitValue(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            bool isCompare = Type == ExpressionType.Compare;
            var leftType = EmitExpressionToken(iLGenerator, Left!, descriptors, !isCompare && !IsLogicalNegation, out var leftLocalBuilder);
            if (leftType == null && !isCompare)
            {
                EmitConstantExpressionToken(iLGenerator, "true", typeof(bool), !IsLogicalNegation, out leftLocalBuilder);
            }
            if (!isCompare)
            {
                if (IsLogicalNegation)
                {
                    leftLocalBuilder = new EmitLocalBuilderDescriptor(iLGenerator.DeclareLocal(typeof(bool)));
                    ExpressionTokenHelper.EmitOperatorType(iLGenerator, OperatorType.LogicalNegation);
                    iLGenerator.Emit(OpCodes.Stloc, leftLocalBuilder.LocalBuilder);
                }
                return leftLocalBuilder;
            }

            var emitOperator = EmitOperatorDescriptor.Create(leftType!, Compare!.OperatorType, Right!);

            emitOperator!.PreEmitOperator(iLGenerator);

            var rightType = EmitExpressionToken(iLGenerator, Right!, descriptors, false, out var rightLocalBuilder);
            if (rightType == null && emitOperator == null)
            {
                EmitConstantExpressionToken(iLGenerator, "true", typeof(bool), false, out rightLocalBuilder);
            }

            emitOperator!.PostEmitOperator(iLGenerator);

            //leftLocalBuilder.EmitValue(iLGenerator, false);
            //rightLocalBuilder!.EmitValue(iLGenerator, false);
            LocalBuilder? localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            //iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
            return new EmitLocalBuilderDescriptor(localBuilder);
        }


        #region private

        private static Type? EmitExpressionToken(ILGenerator iLGenerator, ExpressionTokenDescriptor tokenDescriptor, IList<EmitFieldBuilderDescriptor> descriptors, bool declareLocal, out EmitLocalBuilderDescriptor? descriptor)
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
            IList<EmitFieldBuilderDescriptor> descriptors,
            bool declareLocal,
            out EmitLocalBuilderDescriptor? descriptor
            )
        {
            descriptor = null;

            var emitCallPropertyDescriptor = tokenDescriptor.EmitCallPropertyDescriptor ?? ExpressionTokenHelper.GetEmitCallPropertyDescriptor(tokenDescriptor.Token, descriptors);
            if (emitCallPropertyDescriptor == null)
            {
                return null;
            }

            var lastDescriptor = emitCallPropertyDescriptor.EmitValue(iLGenerator);
            if (declareLocal)
            {
                var localBuilder = iLGenerator.DeclareLocal(emitCallPropertyDescriptor.EmitValueType);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                descriptor = new EmitLocalBuilderDescriptor(localBuilder);
            }
            return lastDescriptor.EmitValueType;
        }


        private static Type? EmitConstantExpressionToken(ILGenerator iLGenerator, string value, Type type, bool declareLocal, out EmitLocalBuilderDescriptor? descriptor)
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
                descriptor = new EmitLocalBuilderDescriptor(localBuilder);
            }
            return type;
        }


        #endregion

    }
}
