﻿using SpringCaching.Parsing;
using SpringCaching.Reflection;
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
        public static ExpressionToken[] ParseExpressionTokens(string expression)
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
            return tokens;
        }

        private static List<EmitPropertyDescriptor> GetEmitPropertyDescriptors(
            ExpressionToken token,
            IList<FieldBuilderDescriptor> descriptors,
            out FieldBuilderDescriptor? fieldDescriptor
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
                        return null;
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

        public static CallPropertyDescriptor? GetCallPropertyDescriptor(ExpressionToken token, IList<FieldBuilderDescriptor> descriptors)
        {
            var propertyDescriptors = GetEmitPropertyDescriptors(token, descriptors, out var fieldDescriptor);
            if (fieldDescriptor == null)
            {
                return null;
            }
            return new CallPropertyDescriptor(fieldDescriptor, propertyDescriptors);
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
                    iLGenerator.Emit(OpCodes.Clt_Un);
                    break;
                case OperatorType.GreaterThan:
                    iLGenerator.Emit(OpCodes.Cgt);
                    break;
                case OperatorType.GreaterThanOrEqual:
                    iLGenerator.Emit(OpCodes.Cgt_Un);
                    break;
                case OperatorType.Equal:
                    iLGenerator.Emit(OpCodes.Ceq);
                    break;
                case OperatorType.NotEqual:
                    iLGenerator.Emit(OpCodes.Cgt_Un);
                    break;
                case OperatorType.BitwiseAnd:
                    break;
                case OperatorType.BitwiseOr:
                    break;
                case OperatorType.LogicalAnd:
                    iLGenerator.Emit(OpCodes.And);
                    break;
                case OperatorType.LogicalOr:
                    iLGenerator.Emit(OpCodes.Or);
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