using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SpringCaching.Infrastructure;
using SpringCaching.Reflection.Emit;
using SpringCaching.Requirement;

namespace SpringCaching.Reflection
{

    internal abstract class AttributeGenerator
    {
        public abstract bool Build(TypeBuilder typeBuilder, Type attributeType, IList<Attribute> attributes, IList<EmitFieldBuilderDescriptor> descriptors);


        protected void SetDefaultProperty(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheBaseAttribute attribute, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            //Key
            if (attribute.Key != null)
            {
                iLGenerator.EmitSetProperty(typeof(CacheRequirementBase).GetProperty("Key")!, attribute.Key, true);
            }
            //Condition
            if (attribute.Condition != null)
            {
                iLGenerator.EmitSetProperty(typeof(CacheRequirementBase).GetProperty("Condition")!, attribute.Condition, true);
            }

            #region KeyGenerator
            if (attribute.KeyGenerator != null)
            {
                if (!typeof(IKeyGenerator).IsAssignableFrom(attribute.KeyGenerator))
                {
                    throw new ArgumentException($"Type {attribute.KeyGenerator.FullName} must be implementation from IKeyGenerator!");
                }
                if (!attribute.KeyGenerator.IsPublic)
                {
                    throw new ArgumentException($"Type {attribute.KeyGenerator.FullName} must be public!");
                }
                //new xxxxKeyGenerator()
                var keyGeneratorConstructor = attribute.KeyGenerator.GetConstructorEx(Type.EmptyTypes);
                if (keyGeneratorConstructor == null)
                {
                    throw new ArgumentException($"Type {attribute.KeyGenerator.FullName} has no default constructor!");
                }
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheRequirementBase).GetProperty("KeyGenerator")!.SetMethod!);
                iLGenerator.Emit(OpCodes.Nop);
            }
            else if (descriptors.Count > 0)
            {
                // KeyGenerator
                iLGenerator.Emit(OpCodes.Dup);
                EmitKeyGenerator(typeBuilder, index, iLGenerator, attribute, descriptors);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheRequirementBase).GetProperty("KeyGenerator")!.SetMethod!);
                iLGenerator.Emit(OpCodes.Nop);
            }
            #endregion

            #region ConditionGenerator
            if (attribute.ConditionGenerator != null)
            {
                if (!typeof(IPredicateGenerator).IsAssignableFrom(attribute.ConditionGenerator))
                {
                    throw new ArgumentException($"Type {attribute.ConditionGenerator.FullName} must be implementation from IPredicateGenerator!");
                }
                if (!attribute.ConditionGenerator.IsPublic)
                {
                    throw new ArgumentException($"Type {attribute.ConditionGenerator.FullName} must be public!");
                }
                //new xxxxConditionGenerator()
                var conditionGeneratorConstructor = attribute.ConditionGenerator.GetConstructorEx(Type.EmptyTypes);
                if (conditionGeneratorConstructor == null)
                {
                    throw new ArgumentException($"Type {attribute.ConditionGenerator.FullName} has no default constructor!");
                }
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Newobj, conditionGeneratorConstructor);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheRequirementBase).GetProperty("ConditionGenerator")!.SetMethod!);
                iLGenerator.Emit(OpCodes.Nop);
            }
            else if (!string.IsNullOrWhiteSpace(attribute.Condition) && descriptors.Count > 0)
            {
                //ConditionGenerator
                iLGenerator.Emit(OpCodes.Dup);
                EmitConditionGenerator(typeBuilder, index, iLGenerator, attribute, descriptors);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheRequirementBase).GetProperty("ConditionGenerator")!.SetMethod!);
                iLGenerator.Emit(OpCodes.Nop);
            }
            #endregion

        }

        private void EmitKeyGenerator(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheBaseAttribute attribute, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (string.IsNullOrWhiteSpace(attribute.Key))
            {
                EmitSimpleKeyGenerator(iLGenerator, descriptors);
                return;
            }
            var keyMethodBuilder = DefineGetKeyMethod(typeBuilder, index, attribute, descriptors);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldftn, keyMethodBuilder);
            //new FuncPredicateGenerator(invoker)
            iLGenerator.Emit(OpCodes.Newobj, typeof(Func<string>).GetConstructorEx());
            iLGenerator.Emit(OpCodes.Newobj, typeof(FuncKeyGenerator).GetConstructorEx());
        }

        private MethodBuilder DefineGetKeyMethod(TypeBuilder typeBuilder, int index, CacheBaseAttribute attribute, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            //create method
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            string methodName = "Get" + attribute.GetType().Name.Replace("Attribute", "") + "Key_" + index;
            var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, typeof(string), Type.EmptyTypes);
            var iLGenerator = methodBuilder.GetILGenerator();
            var emitExpressionResult = StringExpressionGenerator.EmitExpression(iLGenerator, attribute.Key!, descriptors);
            if (emitExpressionResult.Succeed)
            {
                if (emitExpressionResult.LocalBuilder != null)
                {
                    iLGenerator.Emit(OpCodes.Ldloc, emitExpressionResult.LocalBuilder);
                }
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private void EmitConditionGenerator(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheBaseAttribute attribute, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (string.IsNullOrWhiteSpace(attribute.Condition))
            {
                iLGenerator.Emit(OpCodes.Ldnull);
                return;
            }
            var conditionMethodBuilder = DefineGetConditionMethod(typeBuilder, index, attribute, descriptors);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldftn, conditionMethodBuilder);
            //new FuncPredicateGenerator(invoker)
            iLGenerator.Emit(OpCodes.Newobj, typeof(Func<bool>).GetConstructorEx());
            iLGenerator.Emit(OpCodes.Newobj, typeof(FuncPredicateGenerator).GetConstructorEx());
        }

        private MethodBuilder DefineGetConditionMethod(TypeBuilder typeBuilder, int index, CacheBaseAttribute attribute, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            //create method
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            string methodName = "Get" + attribute.GetType().Name.Replace("Attribute", "") + "Condition_" + index;
            var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, typeof(bool), Type.EmptyTypes);
            var iLGenerator = methodBuilder.GetILGenerator();
            var emitExpressionResult = BooleanExpressionGenerator.EmitExpression(iLGenerator, attribute.Condition!, descriptors);
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


        private void EmitSimpleKeyGenerator(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (descriptors.Count == 0)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
                return;
            }

            if (descriptors.Count == 1)
            {
                // xx.ToString()
                var descriptor = descriptors[0];
                var parameterType = descriptor.Parameter.ParameterType;
                if (parameterType == typeof(string))
                {
                    //new SimpleKeyGenerator.StringKeyGenerator
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.StringKeyGenerator).GetConstructorEx();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                else if (parameterType.IsNullableType() && parameterType.GenericTypeArguments[0].IsPrimitive)
                {
                    //new SimpleKeyGenerator.NullableToStringKeyGenerator<T>
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.NullableToStringKeyGenerator<>).MakeGenericType(parameterType.GenericTypeArguments[0]).GetConstructorEx();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                else if (parameterType.IsPrimitive || parameterType.IsValueType)
                {
                    //new SimpleKeyGenerator.StructToStringKeyGenerator<T>
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.StructToStringKeyGenerator<>).MakeGenericType(parameterType).GetConstructorEx();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                //else if (typeof(IEnumerable<>).IsAssignableFrom(parameterType))
                //{ 

                //}
                //else if (typeof(IEnumerable).IsAssignableFrom(parameterType))
                //{

                //}
                else
                {
                    //new SimpleKeyGenerator.JsonKeyGenerator<>
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.JsonKeyGenerator<>).MakeGenericType(parameterType).GetConstructorEx();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                return;
            }

            //new object[]{x,x,x,x,x,}
            iLGenerator.Emit(OpCodes.Ldc_I4, descriptors.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(object));
            int index = 0;
            foreach (var descriptor in descriptors)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                descriptor.EmitValue(iLGenerator);
                descriptor.EmitBox(iLGenerator);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            //new SimpleKeyGenerator.JsonKeyGenerator<>
            var jsonKeyGeneratorConstructor = typeof(SimpleKeyGenerator.JsonKeyGenerator<>).MakeGenericType(typeof(object[])).GetConstructorEx();
            iLGenerator.Emit(OpCodes.Newobj, jsonKeyGeneratorConstructor);
        }

    }

}
