using SpringCaching.Internal;
using SpringCaching.Parsing;
using SpringCaching.Proxy;
using SpringCaching.Reflection.Emit;
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

        public static EmitExpressionResult EmitExpression(ILGenerator iLGenerator, string expression, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            var parsedTokens = ExpressionTokenHelper.ParseExpressionTokens(expression);

            List<EmitStringLocalBuilderDescriptor> tokenLocalBuilders = new List<EmitStringLocalBuilderDescriptor>();
            foreach (var parsedToken in parsedTokens)
            {
                var tokenLocalBuilder = EmitStringExpressionToken(iLGenerator, parsedToken, descriptors);
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
            return EmitExpressionResult.Success();
        }
        private static EmitStringLocalBuilderDescriptor? EmitStringExpressionToken(ILGenerator iLGenerator, ParsedExpressionToken parsedToken, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            switch (parsedToken.Token.TokenType)
            {
                case ExpressionTokenType.Operator:
                    break;
                case ExpressionTokenType.Function:
                    break;
                case ExpressionTokenType.Comma:
                    break;
                case ExpressionTokenType.Field:
                    return EmitStringFieldExpressionToken(iLGenerator, parsedToken.Token, descriptors);
                case ExpressionTokenType.SingleQuoted:
                case ExpressionTokenType.DoubleQuoted:
                    return EmitStringConstantExpressionToken(iLGenerator, parsedToken.Token);
                case ExpressionTokenType.Value:
                    break;
                default:
                    return null;
            }
            return null;
        }

        private static EmitStringLocalBuilderDescriptor? EmitStringFieldExpressionToken(ILGenerator iLGenerator, ExpressionToken token, IList<EmitFieldBuilderDescriptor> descriptors)
        {

            var emitCallPropertyDescriptor = ExpressionTokenHelper.GetEmitCallPropertyDescriptor(token, descriptors);
            if (emitCallPropertyDescriptor == null)
            {
                return null;
            }

            iLGenerator.Emit(OpCodes.Ldarg_0);
            emitCallPropertyDescriptor.EmitValue(iLGenerator);
            EmitToString(iLGenerator, emitCallPropertyDescriptor.EmitValueDescriptor);
            var localBuilder = iLGenerator.DeclareLocal(typeof(string));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return new EmitStringLocalBuilderDescriptor(localBuilder, null);
        }
        private static EmitStringLocalBuilderDescriptor? EmitStringConstantExpressionToken(ILGenerator iLGenerator, ExpressionToken token)
        {
            iLGenerator.Emit(OpCodes.Ldstr, token.Value!);
            var localBuilder = iLGenerator.DeclareLocal(typeof(string));
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return new EmitStringLocalBuilderDescriptor(localBuilder, "null");
        }

        private static void EmitToString(ILGenerator iLGenerator, EmitValueDescriptor descriptor)
        {
            MethodInfo method;
            var type = descriptor.EmitValueType;
            if (type == typeof(string))
            {
                method = typeof(SpringCachingRequirementProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => !o.IsGenericMethod && o.Name == "ToStringFromString").FirstOrDefault()!;
            }
            else if (type.IsNullableType() && type.GenericTypeArguments[0].IsPrimitive)
            {
                method = typeof(SpringCachingRequirementProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.Name == "ToStringFromNullable").FirstOrDefault()!;
                method = method.MakeGenericMethod(type.GenericTypeArguments[0]);
            }
            else if (type.IsPrimitive || type.IsValueType)
            {
                method = typeof(SpringCachingRequirementProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.Name == "ToStringFromStruct").FirstOrDefault()!;
            }
            else
            {
                method = typeof(SpringCachingRequirementProxy).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(o => o.IsGenericMethod && o.Name == "ToStringFromClass").FirstOrDefault()!;
            }

            if (method.IsGenericMethodDefinition)
            {
                method = method.MakeGenericMethod(type);
            }

            iLGenerator.Emit(OpCodes.Call, method);
        }

        private static void EmitConcatString(ILGenerator iLGenerator, IList<EmitStringLocalBuilderDescriptor> stringDescriptors)
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
