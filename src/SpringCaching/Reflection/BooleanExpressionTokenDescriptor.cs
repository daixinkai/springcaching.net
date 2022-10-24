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

        public ParsedExpressionToken? OpenParenthesisToken { get; private set; }
        public ParsedExpressionToken? CloseParenthesisToken { get; private set; }

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

        /// <summary>
        /// !xxxx
        /// </summary>
        public bool IsLogicalNegation { get; set; }
        /// <summary>
        /// xxx||
        /// </summary>
        public bool IsLogicalOr { get; set; }

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

        public static List<BooleanExpressionTokenDescriptor> FromTokens(IList<ParsedExpressionToken> parsedTokens, IList<EmitFieldBuilderDescriptor>? fieldBuilderDescriptors)
        {
            List<BooleanExpressionTokenDescriptor> descriptors = new List<BooleanExpressionTokenDescriptor>();

            BooleanExpressionTokenDescriptor currentDescriptor = new BooleanExpressionTokenDescriptor();

            foreach (var parsedToken in parsedTokens)
            {
                var token = parsedToken.Token;
                var nextToken = parsedToken.NextToken;
                if (token.TokenType == ExpressionTokenType.OpenParenthesis)
                {
                    if (currentDescriptor!.IsCompleted)
                    {
                        descriptors.Add(currentDescriptor);
                    }
                    currentDescriptor = new BooleanExpressionTokenDescriptor();
                    currentDescriptor.OpenParenthesisToken = parsedToken;
                }
                else if (token.TokenType == ExpressionTokenType.CloseParenthesis)
                {
                    if (currentDescriptor!.IsCompleted)
                    {
                        descriptors.Add(currentDescriptor);
                        currentDescriptor.CloseParenthesisToken = parsedToken;
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
                    if (token.OperatorType == OperatorType.LogicalAnd)
                    {
                        if (currentDescriptor.IsCompleted)
                        {
                            descriptors.Add(currentDescriptor);
                        }
                        currentDescriptor = new BooleanExpressionTokenDescriptor();
                    }
                    else if (token.OperatorType == OperatorType.LogicalOr)
                    {
                        if (currentDescriptor.IsCompleted)
                        {
                            descriptors.Add(currentDescriptor);
                        }
                        currentDescriptor = new BooleanExpressionTokenDescriptor();
                        currentDescriptor.IsLogicalOr = true;
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

        public EmitLocalBuilderDescriptor? EmitValue(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (Type == ExpressionType.Compare)
            {
                return EmitCompare(iLGenerator, descriptors);
            }
            bool declareLocal = !IsLogicalNegation;
            //declareLocal = false;
            var leftEmitResult = EmitExpressionToken(iLGenerator, Left!, descriptors, declareLocal);
            if (!leftEmitResult.Succeed)
            {
                leftEmitResult = EmitConstantExpressionToken(iLGenerator, "true", typeof(bool), declareLocal);
            }

            var localBuilder = leftEmitResult.LocalBuilder;
            if (IsLogicalNegation)
            {
                localBuilder = iLGenerator.DeclareLocal(typeof(bool));
                ExpressionTokenHelper.EmitOperatorType(iLGenerator, OperatorType.LogicalNegation);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            }
            return new EmitLocalBuilderDescriptor(localBuilder!);
        }

        public EmitLocalBuilderDescriptor? EmitCompare(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            var leftEmitResult = EmitExpressionToken(iLGenerator, Left!, descriptors, false);
            var emitOperator = EmitOperatorDescriptor.Create(leftEmitResult.Type!, Compare!.OperatorType, Right!);
            emitOperator!.PreEmitOperator(iLGenerator);
            var rightEmitResult = EmitExpressionToken(iLGenerator, Right!, descriptors, false);
            if (!rightEmitResult.Succeed && emitOperator == null)
            {
                EmitConstantExpressionToken(iLGenerator, "true", typeof(bool), false);
            }
            emitOperator!.PostEmitOperator(iLGenerator);
            LocalBuilder? localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return new EmitLocalBuilderDescriptor(localBuilder);
        }

        #region private

        private static EmitExpressionResult EmitExpressionToken(ILGenerator iLGenerator, ExpressionTokenDescriptor tokenDescriptor, IList<EmitFieldBuilderDescriptor> descriptors, bool declareLocal)
        {
            switch (tokenDescriptor.TokenType)
            {
                case ExpressionTokenType.Operator:
                    break;
                case ExpressionTokenType.Function:
                    break;
                case ExpressionTokenType.Comma:
                    break;
                case ExpressionTokenType.Field:
                    return EmitFieldExpressionToken(iLGenerator, tokenDescriptor, descriptors, declareLocal);
                case ExpressionTokenType.SingleQuoted:
                case ExpressionTokenType.DoubleQuoted:
                    return EmitConstantExpressionToken(iLGenerator, tokenDescriptor.Token.Value!, typeof(string), declareLocal);
                case ExpressionTokenType.Value:
                    return EmitConstantExpressionToken(iLGenerator, tokenDescriptor.Token.Value!, typeof(int), declareLocal);
                default:
                    return EmitExpressionResult.Fail();
            }
            return EmitExpressionResult.Fail();
        }

        private static EmitExpressionResult EmitFieldExpressionToken(
            ILGenerator iLGenerator,
            ExpressionTokenDescriptor tokenDescriptor,
            IList<EmitFieldBuilderDescriptor> descriptors,
            bool declareLocal
            )
        {
            var emitCallPropertyDescriptor = tokenDescriptor.EmitCallPropertyDescriptor ?? ExpressionTokenHelper.GetEmitCallPropertyDescriptor(tokenDescriptor.Token, descriptors);
            if (emitCallPropertyDescriptor == null)
            {
                return EmitExpressionResult.Fail();
            }

            var lastDescriptor = emitCallPropertyDescriptor.EmitValue(iLGenerator);
            if (declareLocal)
            {
                var localBuilder = iLGenerator.DeclareLocal(emitCallPropertyDescriptor.EmitValueType);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                return EmitExpressionResult.Success(localBuilder);
            }
            return EmitExpressionResult.Success(lastDescriptor.EmitValueType);
        }


        private static EmitExpressionResult EmitConstantExpressionToken(ILGenerator iLGenerator, string value, Type type, bool declareLocal)
        {
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
                return EmitExpressionResult.Fail();
            }
            if (declareLocal)
            {
                var localBuilder = iLGenerator.DeclareLocal(type);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                return EmitExpressionResult.Success(localBuilder);
            }
            return EmitExpressionResult.Success(type);
        }


        #endregion

    }
}
