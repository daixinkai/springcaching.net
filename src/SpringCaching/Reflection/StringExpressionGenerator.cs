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
    internal static class StringExpressionGenerator
    {

        public static EmitExpressionResult EmitExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> descriptors)
        {
            var tokens = ExpressionTokenHelper.ParseExpressionTokens(expression);

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
                return EmitExpressionResult.Fail();
            }
            if (tokenLocalBuilders.Count == 1)
            {
                return EmitExpressionResult.Success(tokenLocalBuilders[0].LocalBuilder);
            }
            //localBuilder = iLGenerator.DeclareLocal(typeof(string));
            EmitConcatString(iLGenerator, tokenLocalBuilders);
            //iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return EmitExpressionResult.Success(null);
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

            var callPropertyDescriptor = ExpressionTokenHelper.GetCallPropertyDescriptor(token, descriptors);
            if (callPropertyDescriptor == null)
            {
                return null;
            }

            bool toString = callPropertyDescriptor.EmitValueType != typeof(string);
            if (toString)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
            }
            callPropertyDescriptor.EmitValue(iLGenerator);
            bool canBeNull = true;
            if (toString)
            {
                EmitToString(iLGenerator, callPropertyDescriptor.EmitValueDescriptor, out canBeNull);
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
                stringDescriptors[0].EmitValue(iLGenerator);
                return;
            }
            var method = typeof(string).GetMethod("Concat", stringDescriptors.Select(s => typeof(string)).ToArray());
            if (method != null)
            {
                foreach (var descriptor in stringDescriptors)
                {
                    descriptor.EmitValue(iLGenerator);
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
                descriptor.EmitValue(iLGenerator);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Call, method!);
        }


    }
}
