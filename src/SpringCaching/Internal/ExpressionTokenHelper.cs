using SpringCaching.Parsing;
using SpringCaching.Reflection;
using SpringCaching.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Internal
{
    internal static class ExpressionTokenHelper
    {
        public static ParsedExpressionToken[] ParseExpressionTokens(string expression)
        {
            var infixTokenizer = new InfixTokenizer();
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
            List<ParsedExpressionToken> parsedTokens = new List<ParsedExpressionToken>();

            ArrayEx.ForEach(tokens, (current, next) =>
            {
                ParsedExpressionToken parsedToken = new ParsedExpressionToken(current);
                parsedToken.NextToken = next;
                parsedTokens.Add(parsedToken);
            });
            return parsedTokens.ToArray();
        }

        private static List<EmitPropertyDescriptor> GetEmitPropertyDescriptors(
            ExpressionToken token,
            IList<EmitFieldBuilderDescriptor> descriptors,
            out EmitFieldBuilderDescriptor? fieldDescriptor
            )
        {
            string value = token.Value!;
            List<string> fieldList;
            bool checkFieldNull = false;
            if (value.Contains('.'))
            {
                fieldList = value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                string fieldName = fieldList[0];
                checkFieldNull = fieldName.EndsWith("?");
                if (checkFieldNull)
                {
                    fieldName = fieldName.TrimEnd('?');
                }
                fieldDescriptor = descriptors.FirstOrDefault(s => s.Parameter.Name == fieldName);
                fieldList.RemoveAt(0);
            }
            else
            {
                fieldDescriptor = descriptors.FirstOrDefault(s => s.Parameter.Name == value);
                fieldList = new List<string>();
            }

            var propertyDescriptors = new List<EmitPropertyDescriptor>();

            if (fieldDescriptor == null)
            {
                return propertyDescriptors;
            }


            if (fieldList.Count > 0)
            {
                bool lastCheckNull = checkFieldNull;
                Type propertyType = fieldDescriptor.Parameter.ParameterType;
                //property
                foreach (var item in fieldList)
                {
                    string propertyName = item;
                    bool checkNull = propertyName.EndsWith("?");
                    if (checkNull)
                    {
                        propertyName = propertyName.TrimEnd('?');
                    }
                    var property = propertyType.GetProperty(propertyName);
                    if (property == null || !property.CanRead)
                    {
                        return propertyDescriptors;
                    }
                    propertyDescriptors.Add(new EmitPropertyDescriptor(property, !lastCheckNull));
                    propertyType = property.PropertyType;
                    lastCheckNull = checkNull;
                }
            }

            if (propertyDescriptors.Count > 0)
            {
                propertyDescriptors[propertyDescriptors.Count - 1].IsLast = true;
            }

            return propertyDescriptors;
        }

        public static EmitCallPropertyDescriptor? GetEmitCallPropertyDescriptor(ExpressionToken token, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            var propertyDescriptors = GetEmitPropertyDescriptors(token, descriptors, out var fieldDescriptor);
            if (fieldDescriptor == null)
            {
                return null;
            }
            return new EmitCallPropertyDescriptor(fieldDescriptor, propertyDescriptors);
        }

        public static void EmitOperatorType(ILGenerator iLGenerator, OperatorType operatorType)
        {
            switch (operatorType)
            {
                case OperatorType.None:
                    break;
                case OperatorType.PostIncrement:
                    break;
                case OperatorType.PostDecrement:
                    break;
                case OperatorType.PreIncrement:
                    break;
                case OperatorType.PreDecrement:
                    break;
                case OperatorType.UnaryPlus:
                    break;
                case OperatorType.UnaryMinus:
                    break;
                case OperatorType.LogicalNegation:
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    iLGenerator.Emit(OpCodes.Ceq);
                    break;
                case OperatorType.Multiplication:
                    break;
                case OperatorType.Division:
                    break;
                case OperatorType.Modulus:
                    break;
                case OperatorType.Addition:
                    break;
                case OperatorType.Subtraction:
                    break;
                case OperatorType.LessThan:
                    iLGenerator.Emit(OpCodes.Clt);
                    break;
                case OperatorType.LessThanOrEqual:
                    iLGenerator.Emit(OpCodes.Cgt);
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    iLGenerator.Emit(OpCodes.Ceq);
                    break;
                case OperatorType.GreaterThan:
                    iLGenerator.Emit(OpCodes.Cgt);
                    break;
                case OperatorType.GreaterThanOrEqual:
                    iLGenerator.Emit(OpCodes.Clt);
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    iLGenerator.Emit(OpCodes.Ceq);
                    break;
                case OperatorType.Equal:
                    iLGenerator.Emit(OpCodes.Ceq);
                    break;
                case OperatorType.NotEqual:
                    iLGenerator.Emit(OpCodes.Cgt_Un);
                    break;
                case OperatorType.BitwiseAnd:
                    iLGenerator.Emit(OpCodes.And);
                    break;
                case OperatorType.BitwiseOr:
                    iLGenerator.Emit(OpCodes.Or);
                    break;
                case OperatorType.LogicalAnd:
                    break;
                case OperatorType.LogicalOr:
                    break;
                case OperatorType.Assignment:
                    break;
                case OperatorType.AdditionAssignment:
                    break;
                case OperatorType.SubtractionAssignment:
                    break;
                case OperatorType.MultiplicationAssignment:
                    break;
                case OperatorType.DivisionAssignment:
                    break;
                case OperatorType.ModulusAssignment:
                    break;
                default:
                    break;
            }
        }


    }
}
