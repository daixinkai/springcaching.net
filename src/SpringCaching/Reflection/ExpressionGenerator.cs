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

            List<StringLocalBuilderDescriptor> tokenLocalBuilders = new List<StringLocalBuilderDescriptor>();
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
        private static StringLocalBuilderDescriptor? EmitStringExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> descriptors)
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

        private static StringLocalBuilderDescriptor? EmitStringFieldExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> descriptors)
        {
            string value = token.Value!;
            FieldBuilderDescriptor? fieldDescriptor;
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
            if (fieldDescriptor == null)
            {
                return null;
            }
            var propertyDescriptors = new List<EmitPropertyDescriptor>();
            bool hasCheckNull = false;
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
                        hasCheckNull = true;
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

            EmitValueDescriptor emitValueDescriptor = propertyDescriptors.Count > 0 ?
                propertyDescriptors[propertyDescriptors.Count - 1] :
                fieldDescriptor;

            bool toString = emitValueDescriptor.EmitValueType != typeof(string);
            if (toString)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
            }
            EmitPropertyDescriptor.EmitValue(iLGenerator, fieldDescriptor, propertyDescriptors);
            bool canBeNull = true;
            if (toString)
            {
                EmitToString(iLGenerator, emitValueDescriptor, out canBeNull);
            }
            var localBuilder = iLGenerator.DeclareLocal(typeof(string));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return new StringLocalBuilderDescriptor(localBuilder, canBeNull);
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

        private static void EmitToString(ILGenerator iLGenerator, EmitValueDescriptor descriptor, out bool canBeNull)
        {
            canBeNull = true;
            MethodInfo method;
            var type = descriptor.EmitValueType;
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

            //if (descriptor is EmitPropertyDescriptor propertyDescriptor && propertyDescriptor.LocalBuilder != null)
            //{
            //    Label callLabel = iLGenerator.DefineLabel();
            //    Label isNullLabel = iLGenerator.DefineLabel();
            //    iLGenerator.Emit(OpCodes.Dup);
            //    iLGenerator.Emit(OpCodes.Brtrue_S, isNullLabel);
            //    iLGenerator.Emit(OpCodes.Pop);
            //    iLGenerator.Emit(OpCodes.Ldnull);
            //    iLGenerator.Emit(OpCodes.Br_S, callLabel);
            //    iLGenerator.MarkLabel(isNullLabel);
            //    iLGenerator.Emit(OpCodes.Ldloc, propertyDescriptor.LocalBuilder);
            //    iLGenerator.MarkLabel(callLabel);
            //    iLGenerator.Emit(OpCodes.Ldnull);

            //}

            iLGenerator.Emit(OpCodes.Call, method);

        }

        private static void EmitConcatString(ILGenerator iLGenerator, IList<StringLocalBuilderDescriptor> stringDescriptors)
        {
            if (stringDescriptors.Count == 1)
            {
                stringDescriptors[0].EmitValue(iLGenerator, false);
                return;
            }
            var method = typeof(string).GetMethod("Concat", stringDescriptors.Select(s => typeof(string)).ToArray());
            if (method != null)
            {
                foreach (var descriptor in stringDescriptors)
                {
                    descriptor.EmitValue(iLGenerator, false);
                }
                iLGenerator.Emit(OpCodes.Call, method);
                return;
            }
            method = typeof(string).GetMethod("Concat", new Type[] { typeof(string[]) });
            //new string[]{x,x,x,x,x,}
            iLGenerator.Emit(OpCodes.Ldc_I4, stringDescriptors.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(string));
            int index = 0;
            foreach (var descriptor in stringDescriptors)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                descriptor.EmitValue(iLGenerator, false);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Call, method!);
        }

    }
}
