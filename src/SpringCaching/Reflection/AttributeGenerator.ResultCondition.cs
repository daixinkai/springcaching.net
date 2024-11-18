using SpringCaching.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpringCaching.Infrastructure;
using SpringCaching.Reflection.Emit;

namespace SpringCaching.Reflection
{
    partial class AttributeGenerator
    {
        protected void SetResultConditionProperty(TypeBuilder typeBuilder, MethodInfo methodInfo, int index, ILGenerator iLGenerator, IResultConditionAttribute attribute, Type requirementType)
        {
            if (attribute.ResultConditionGenerator != null)
            {
                if (!typeof(IPredicateGenerator).IsAssignableFrom(attribute.ResultConditionGenerator))
                {
                    throw new ArgumentException($"Type {attribute.ResultConditionGenerator.FullName} must be implementation from IResultPredicateGenerator!");
                }
                if (!attribute.ResultConditionGenerator.IsPublic && !attribute.ResultConditionGenerator.IsNestedPublic)
                {
                    throw new ArgumentException($"Type {attribute.ResultConditionGenerator.FullName} must be public!");
                }
                //new xxxxConditionGenerator()
                var resultConditionGeneratorConstructor = attribute.ResultConditionGenerator.GetConstructorEx(Type.EmptyTypes);
                if (resultConditionGeneratorConstructor == null)
                {
                    throw new ArgumentException($"Type {attribute.ResultConditionGenerator.FullName} has no default constructor!");
                }
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Newobj, resultConditionGeneratorConstructor);
                iLGenerator.Emit(OpCodes.Callvirt, requirementType.GetProperty("ResultConditionGenerator")!.SetMethod!);
                iLGenerator.EmitNop();
            }
            else if (!string.IsNullOrWhiteSpace(attribute.ResultCondition))
            {
                //ResultConditionGenerator
                iLGenerator.Emit(OpCodes.Dup);
                EmitResultConditionGenerator(typeBuilder, methodInfo, index, iLGenerator, attribute);
                iLGenerator.Emit(OpCodes.Callvirt, requirementType.GetProperty("ResultConditionGenerator")!.SetMethod!);
                iLGenerator.EmitNop();
            }
        }

        private void EmitResultConditionGenerator(TypeBuilder typeBuilder, MethodInfo methodInfo, int index, ILGenerator iLGenerator, IResultConditionAttribute attribute)
        {
            if (string.IsNullOrWhiteSpace(attribute.ResultCondition))
            {
                iLGenerator.Emit(OpCodes.Ldnull);
                return;
            }
            var conditionMethodBuilder = DefineGetResultConditionMethod(typeBuilder, methodInfo, index, attribute);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldftn, conditionMethodBuilder);
            //new FuncPredicateGenerator(invoker)
            iLGenerator.Emit(OpCodes.Newobj, typeof(Func<bool>).GetConstructorEx());
            iLGenerator.Emit(OpCodes.Newobj, typeof(ResultFuncPredicateGenerator<>).MakeGenericType(GetReturnType(methodInfo)).GetConstructorEx());
        }

        private MethodBuilder DefineGetResultConditionMethod(TypeBuilder typeBuilder, MethodInfo methodInfo, int index, IResultConditionAttribute attribute)
        {
            //create method
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            string methodName = "Get" + attribute.GetType().Name.Replace("Attribute", "") + "ResultCondition_" + index;

            var resultType = GetReturnType(methodInfo);

            var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, typeof(bool), new[] { resultType });
            var iLGenerator = methodBuilder.GetILGenerator();
            var parameterBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.None, "result");
            var descriptors = new[] { new EmitParameterBuilderDescriptor(parameterBuilder, resultType) };
            var emitExpressionResult = BooleanExpressionGenerator.EmitExpression(iLGenerator, attribute.ResultCondition!, descriptors);
            if (emitExpressionResult.Succeed)
            {
                if (emitExpressionResult.LocalBuilder != null)
                {
                    iLGenerator.Emit(OpCodes.Ldloc, emitExpressionResult.LocalBuilder);
                }
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldc_I4_1);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private static Type GetReturnType(MethodInfo methodInfo)
        {
            var returnType = methodInfo.ReturnType;
            if (returnType.IsGenericType)
            {
                if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }
                //else if (returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                //{
                //    returnType = returnType.GetGenericArguments()[0];
                //}
            }
            return returnType;
        }
    }
}
