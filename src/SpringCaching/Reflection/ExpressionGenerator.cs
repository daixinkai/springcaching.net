using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public static class ExpressionGenerator
#else
    internal static class ExpressionGenerator
#endif
    {
        public static bool EmitStringExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            return false;
            var tokens = ParseExpressionTokens(expression);
            iLGenerator.Emit(OpCodes.Ldnull);
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(string));
            foreach (var token in tokens)
            {
                if (EmitStringExpressionToken(iLGenerator, token, fieldBuilders))
                {
                    iLGenerator.Emit(OpCodes.Ldloca_S, localBuilder);
                }
                //EmitStringExpressionToken();
            }
            return true;
        }

        public static bool EmitBooleanExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            return false;
            var tokens = ParseExpressionTokens(expression);
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            foreach (var token in tokens)
            {

            }
            return true;
        }

        public static ExpressionToken[] ParseExpressionTokens(string expression)
        {
            InfixTokenizer infixTokenizer = new InfixTokenizer();
            var tokens = infixTokenizer.Tokenize(expression);
            //process value
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token.Value == null)
                {
                    continue;
                }
                if (token.TokenType == ExpressionTokenType.Value && token.Value.StartsWith("#"))
                {
                    //#param.Id
                    // field                    
                    tokens[i] = new ExpressionToken()
                    {
                        TokenType = ExpressionTokenType.Field,
                        OperatorType = token.OperatorType
                    };
                    var rawToken = new KnownRawToken(token.RawToken?.TokenType ?? RawTokenType.None, token.RawToken?.Position ?? -1, token.Value.TrimStart('#'));
                    tokens[i].Append(rawToken);
                    tokens[i].Freeze();
                }
                else if (token.TokenType == ExpressionTokenType.Function && token.Value.StartsWith("#"))
                {
                    //#param.Id.ToString()
                    // function                    
                    tokens[i] = new ExpressionToken()
                    {
                        TokenType = ExpressionTokenType.Function,
                        OperatorType = token.OperatorType
                    };
                    var rawToken = new KnownRawToken(token.RawToken?.TokenType ?? RawTokenType.None, token.RawToken?.Position ?? -1, token.Value.TrimStart('#'));
                    tokens[i].Append(rawToken);
                    tokens[i].Freeze();
                }
                else if (token.TokenType == ExpressionTokenType.SingleQuoted && token.Value.Length > 1)
                {
                    //'Name'
                    tokens[i] = new ExpressionToken()
                    {
                        TokenType = ExpressionTokenType.DoubleQuoted,
                        OperatorType = token.OperatorType
                    };
                    var rawToken = new KnownRawToken(token.RawToken?.TokenType ?? RawTokenType.None, token.RawToken?.Position ?? -1, token.Value);
                    tokens[i].Append(rawToken);
                    tokens[i].Freeze();
                }
            }
            return tokens;
        }


        private static bool EmitStringExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            switch (token.TokenType)
            {
                case ExpressionTokenType.Operator:
                    break;
                case ExpressionTokenType.Function:
                    break;
                case ExpressionTokenType.Comma:
                    break;
                case ExpressionTokenType.Field:
                    break;
                case ExpressionTokenType.SingleQuoted:
                    //iLGenerator.Emit(OpCodes);
                    break;
                case ExpressionTokenType.DoubleQuoted:
                    iLGenerator.Emit(OpCodes.Ldstr, token.Value!);
                    break;
                case ExpressionTokenType.Value:
                    break;
                default:
                    return false;
            }
            return true;
        }

    }
}
