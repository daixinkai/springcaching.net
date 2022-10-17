using SpringCaching.Internal;
using SpringCaching.Parsing;
using SpringCaching.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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
        public static LocalBuilder? EmitStringExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> descriptors)
        {
            var tokens = ParseExpressionTokens(expression);

            List<LocalBuilderDescriptor> tokenLocalBuilders = new List<LocalBuilderDescriptor>();
            foreach (var token in tokens)
            {
                var tokenLocalBuilder = EmitStringExpressionToken(iLGenerator, token, descriptors);
                if (tokenLocalBuilder != null)
                {
                    tokenLocalBuilders.Add(tokenLocalBuilder);
                }
            }

            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(string));

            if (tokenLocalBuilders.Count == 0)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                EmitConcatString(iLGenerator, tokenLocalBuilders);
            }
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return localBuilder;
        }
        private static LocalBuilderDescriptor? EmitStringExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> descriptors)
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
                    return EmitStringFieldExpressionToken(iLGenerator, token, descriptors);
                case ExpressionTokenType.SingleQuoted:
                    //iLGenerator.Emit(OpCodes);
                    break;
                case ExpressionTokenType.DoubleQuoted:
                    iLGenerator.Emit(OpCodes.Ldstr, token.Value!);
                    break;
                case ExpressionTokenType.Value:
                    break;
                default:
                    return null;
            }
            return null;
        }

        private static LocalBuilderDescriptor? EmitStringFieldExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> descriptors)
        {
            string value = token.Value!;
            FieldBuilderDescriptor? descriptor;
            List<string> fieldList;
            if (value.Contains('.'))
            {
                fieldList = value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                descriptor = descriptors.FirstOrDefault(s => s.Parameter.Name == fieldList[0]);
                fieldList.RemoveAt(0);
            }
            else
            {
                descriptor = descriptors.FirstOrDefault(s => s.Parameter.Name == value);
                fieldList = new List<string>();
            }
            if (descriptor == null)
            {
                return null;
            }
            var properties = new List<PropertyInfo>();
            if (fieldList.Count > 0)
            {
                Type propertyType = descriptor.Parameter.ParameterType;
                //property
                foreach (var item in fieldList)
                {
                    var property = propertyType.GetProperty(item);
                    if (property == null || !property.CanRead)
                    {
                        return null;
                    }
                    properties.Add(property);
                    propertyType = property.PropertyType;
                }
            }

            var lastType = properties.Count > 0 ?
                properties[properties.Count - 1].PropertyType :
                descriptor.Parameter.ParameterType;

            bool toString = lastType != typeof(string);

            if (toString)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
            }

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
            foreach (var property in properties)
            {
                iLGenerator.Emit(OpCodes.Callvirt, property.GetMethod!);
            }
            bool canBeNull = true;
            if (toString)
            {
                EmitToString(iLGenerator, lastType, out canBeNull);
            }
            var localBuilder = iLGenerator.DeclareLocal(typeof(string));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return new LocalBuilderDescriptor(localBuilder, canBeNull);
        }

        #endregion

        #region boolean


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



        #endregion

        public static LocalBuilder? EmitBooleanExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> descriptors)
        {
            return null;
            var tokens = ParseExpressionTokens(expression);
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            foreach (var token in tokens)
            {

            }
            return localBuilder;
        }

        private static void EmitToString(ILGenerator iLGenerator, Type type, out bool canBeNull)
        {
            canBeNull = true;
            MethodInfo method;
            if (type.IsNullableType() && type.GenericTypeArguments[0].IsPrimitive)
            {
                method = typeof(SpringCachingRequirementProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.Name == "ToNullableString").FirstOrDefault()!;
                method = method.MakeGenericMethod(type.GenericTypeArguments[0]);
                canBeNull = false;
            }
            else if (type.IsPrimitive || type.IsValueType)
            {
                method = typeof(SpringCachingRequirementProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.Name == "ToStructString").FirstOrDefault()!;
                canBeNull = false;
            }
            else
            {
                method = typeof(SpringCachingRequirementProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.Name == "ToClassString").FirstOrDefault()!;
            }
            if (method.IsGenericMethodDefinition)
            {
                method = method.MakeGenericMethod(type);
            }

            iLGenerator.Emit(OpCodes.Call, method);

            //var method = type.GetMethod("ToString", Type.EmptyTypes);

            //if (method != null)
            //{
            //    iLGenerator.Emit(OpCodes.Call, method);
            //    return;
            //}
            //if (type.IsPrimitive)
            //{

            //}
            //iLGenerator.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
        }

        private static void EmitConcatString(ILGenerator iLGenerator, IList<LocalBuilderDescriptor> descriptors)
        {
            if (descriptors.Count == 1)
            {
                descriptors[0].EmitValue(iLGenerator);
                return;
            }
            var method = typeof(string).GetMethod("Concat", descriptors.Select(s => typeof(string)).ToArray());
            if (method != null)
            {
                foreach (var descriptor in descriptors)
                {
                    descriptor.EmitValue(iLGenerator);
                }
                iLGenerator.Emit(OpCodes.Call, method);
                return;
            }
            method = typeof(string).GetMethod("Concat", new Type[] { typeof(string[]) });
            //new string[]{x,x,x,x,x,}
            iLGenerator.Emit(OpCodes.Ldc_I4, descriptors.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(string));
            int index = 0;
            foreach (var descriptor in descriptors)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                descriptor.EmitValue(iLGenerator);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Call, method!);
        }

    }
}
