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
#if DEBUG
    public static class ExpressionGenerator
#else
    internal static class ExpressionGenerator
#endif
    {

        #region string
        public static LocalBuilder EmitStringExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            return null;
            var tokens = ParseExpressionTokens(expression);
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(string));
            bool setValue = false;
            foreach (var token in tokens)
            {
                if (EmitStringExpressionToken(iLGenerator, token, fieldBuilders))
                {
                    if (!setValue)
                    {
                        iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                        setValue = true;
                        break;
                        iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
                    }
                    else
                    {
                        var concatMethod = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                        iLGenerator.Emit(OpCodes.Call, concatMethod);
                    }
                    //iLGenerator.Emit(OpCodes.Ldloca_S, localBuilder);
                }
            }
            if (!setValue)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            }
            return localBuilder;
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
                    if (!EmitStringFieldExpressionToken(iLGenerator, token, fieldBuilders))
                    {
                        return false;
                    }
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

        private static bool EmitStringFieldExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            string value = token.Value!;
            FieldBuilderDescriptor? fieldBuilder;
            List<string> fieldList;
            if (value.Contains("."))
            {
                fieldList = value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                fieldBuilder = fieldBuilders.FirstOrDefault(s => s.Parameter.Name == fieldList[0]);
                fieldList.RemoveAt(0);
            }
            else
            {
                fieldBuilder = fieldBuilders.FirstOrDefault(s => s.Parameter.Name == value);
                fieldList = new List<string>();
            }
            if (fieldBuilder == null)
            {
                return false;
            }
            List<PropertyInfo> properties = new List<PropertyInfo>();
            if (fieldList.Count > 0)
            {
                Type propertyType = fieldBuilder.Parameter.ParameterType;
                //property
                foreach (var item in fieldList)
                {
                    var property = propertyType.GetProperty(item);
                    if (property == null || !property.CanRead)
                    {
                        return false;
                    }
                    properties.Add(property);
                    propertyType = property.PropertyType;
                }
            }
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, fieldBuilder.FieldBuilder);
            foreach (var property in properties)
            {
                iLGenerator.Emit(OpCodes.Callvirt, property.GetMethod);
            }
            var lastType = properties.Count > 0 ?
                properties[properties.Count - 1].PropertyType :
                fieldBuilder.Parameter.ParameterType;
            if (lastType != typeof(string))
            {
                iLGenerator.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            }
            return true;
        }

        #endregion

        #region boolean


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



        #endregion

        public static LocalBuilder EmitBooleanExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            return null;
            var tokens = ParseExpressionTokens(expression);
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            foreach (var token in tokens)
            {

            }
            return localBuilder;
        }



    }
}
