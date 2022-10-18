using SpringCaching.Internal;
using SpringCaching.Parsing;
using SpringCaching.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            if (tokenLocalBuilders.Count == 0)
            {
                return null;
            }
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(string));
            EmitConcatString(iLGenerator, tokenLocalBuilders);
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
                case ExpressionTokenType.DoubleQuoted:
                    return EmitStringConstantExpressionToken(iLGenerator, token);
                case ExpressionTokenType.Value:
                    break;
                default:
                    return null;
            }
            return null;
        }

        private static StringLocalBuilderDescriptor? EmitStringFieldExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> descriptors)
        {

            var propertyDescriptors = GetEmitPropertyDescriptors(token, descriptors, out var fieldDescriptor);

            if (fieldDescriptor == null)
            {
                return null;
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
            return new StringLocalBuilderDescriptor(localBuilder, canBeNull ? "null" : null);
        }
        private static StringLocalBuilderDescriptor? EmitStringConstantExpressionToken(ILGenerator iLGenerator, ExpressionToken token)
        {
            iLGenerator.Emit(OpCodes.Ldstr, token.Value!);
            var localBuilder = iLGenerator.DeclareLocal(typeof(string));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return new StringLocalBuilderDescriptor(localBuilder, "null");
        }


        #endregion

        #region boolean

        public static LocalBuilder? EmitBooleanExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> descriptors)
        {
            var tokens = ParseExpressionTokens(expression);

            var tokenDescriptors = BooleanExpressionTokenDescriptor.FromTokens(tokens);

            List<LocalBuilderDescriptor> tokenLocalBuilders = new List<LocalBuilderDescriptor>();
            foreach (var tokenDescriptor in tokenDescriptors)
            {
                var tokenLocalBuilder = EmitBooleanExpressionToken(iLGenerator, tokenDescriptor, descriptors);
                if (tokenLocalBuilder != null)
                {
                    tokenLocalBuilders.Add(tokenLocalBuilder);
                }
            }
            if (tokenLocalBuilders.Count == 0)
            {
                return null;
            }
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            //EmitConcatString(iLGenerator, tokenLocalBuilders);
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return localBuilder;
        }

        private static LocalBuilderDescriptor? EmitBooleanExpressionToken(ILGenerator iLGenerator, BooleanExpressionTokenDescriptor tokenDescriptor, IList<FieldBuilderDescriptor> descriptors)
        {
            bool isCompare = tokenDescriptor.Type == BooleanExpressionTokenDescriptor.ExpressionType.Compare;
            if (!EmitBooleanExpressionToken(iLGenerator, tokenDescriptor.Left, descriptors, !isCompare, out var leftLocalBuilder))
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

            if (!EmitBooleanExpressionToken(iLGenerator, tokenDescriptor.Right!, descriptors, false, out var rightLocalBuilder))
            {
                EmitConstantExpressionToken(iLGenerator, "true", typeof(bool), false, out rightLocalBuilder);
            }
            EmitOperatorType(iLGenerator, tokenDescriptor.Compare!.OperatorType);

            var label = iLGenerator.DefineLabel();

            iLGenerator.Emit(OpCodes.Br_S, label);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);

            iLGenerator.MarkLabel(label);

            //leftLocalBuilder.EmitValue(iLGenerator, false);
            //rightLocalBuilder!.EmitValue(iLGenerator, false);
            LocalBuilder? localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            //iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
            return new LocalBuilderDescriptor(localBuilder);
        }

        private static bool EmitBooleanExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<FieldBuilderDescriptor> descriptors, bool declareLocal, out LocalBuilderDescriptor? descriptor)
        {
            descriptor = null;
            switch (token.TokenType)
            {
                case ExpressionTokenType.Operator:
                    break;
                case ExpressionTokenType.Function:
                    break;
                case ExpressionTokenType.Comma:
                    break;
                case ExpressionTokenType.Field:
                    return EmitBooleanFieldExpressionToken(iLGenerator, token, descriptors, declareLocal, out descriptor);
                case ExpressionTokenType.SingleQuoted:
                case ExpressionTokenType.DoubleQuoted:
                    return EmitConstantExpressionToken(iLGenerator, token.Value!, typeof(string), declareLocal, out descriptor);
                case ExpressionTokenType.Value:
                    return EmitConstantExpressionToken(iLGenerator, token.Value!, typeof(int), declareLocal, out descriptor);
                default:
                    return false;
            }
            return false;
        }

        private static bool EmitBooleanFieldExpressionToken(
            ILGenerator iLGenerator,
            ExpressionToken token,
            IList<FieldBuilderDescriptor> descriptors,
            bool declareLocal,
            out LocalBuilderDescriptor? descriptor
            )
        {
            descriptor = null;
            var propertyDescriptors = GetEmitPropertyDescriptors(token, descriptors, out var fieldDescriptor);

            if (fieldDescriptor == null)
            {
                return false;
            }

            EmitValueDescriptor emitValueDescriptor = propertyDescriptors.Count > 0 ?
                propertyDescriptors[propertyDescriptors.Count - 1] :
                fieldDescriptor;
            EmitPropertyDescriptor.EmitValue(iLGenerator, fieldDescriptor, propertyDescriptors);
            if (declareLocal)
            {
                var localBuilder = iLGenerator.DeclareLocal(emitValueDescriptor.EmitValueType);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                descriptor = new LocalBuilderDescriptor(localBuilder);
            }
            return true;
        }


        #endregion


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

        private static bool EmitConstantExpressionToken(ILGenerator iLGenerator, string value, Type type, bool declareLocal, out LocalBuilderDescriptor? descriptor)
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
                return false;
            }
            if (declareLocal)
            {
                var localBuilder = iLGenerator.DeclareLocal(type);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
                descriptor = new LocalBuilderDescriptor(localBuilder);
            }
            return true;
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

        private static void EmitOperatorType(ILGenerator iLGenerator, OperatorType operatorType)
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
                    break;
                case OperatorType.NotEqual:
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
