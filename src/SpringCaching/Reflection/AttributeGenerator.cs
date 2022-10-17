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
using SpringCaching.Requirement;

namespace SpringCaching.Reflection
{

    internal abstract class AttributeGenerator
    {
        public abstract bool Build(TypeBuilder typeBuilder, Type attributeType, IList<Attribute> attributes, IList<FieldBuilderDescriptor> descriptors);


        protected void SetDefaultProperty(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheBaseAttribute attribute, IList<FieldBuilderDescriptor> descriptors)
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

            if (descriptors.Count > 0)
            {
                // KeyGenerator
                iLGenerator.Emit(OpCodes.Dup);
                EmitKeyGenerator(typeBuilder, index, iLGenerator, attribute, descriptors);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheRequirementBase).GetProperty("KeyGenerator")!.SetMethod!);
                iLGenerator.Emit(OpCodes.Nop);
            }

            if (!string.IsNullOrWhiteSpace(attribute.Condition) && descriptors.Count > 0)
            {
                //ConditionGenerator
                iLGenerator.Emit(OpCodes.Dup);
                EmitConditionGenerator(typeBuilder, index, iLGenerator, attribute, descriptors);
                iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheRequirementBase).GetProperty("ConditionGenerator")!.SetMethod!);
                iLGenerator.Emit(OpCodes.Nop);
            }

        }

        private void EmitKeyGenerator(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheBaseAttribute attribute, IList<FieldBuilderDescriptor> descriptors)
        {
            if (string.IsNullOrWhiteSpace(attribute.Key))
            {
                EmitSimpleKeyGenerator(iLGenerator, descriptors);
                return;
            }
            var methodBuilder = DefineGetKeyGeneratorMethod(typeBuilder, index, attribute, descriptors);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, methodBuilder);
        }

        private MethodBuilder DefineGetKeyGeneratorMethod(TypeBuilder typeBuilder, int index, CacheBaseAttribute attribute, IList<FieldBuilderDescriptor> descriptors)
        {
            //create method
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            string methodName = "Get" + attribute.GetType().Name.Replace("Attribute", "") + "KeyGenerator_" + index;
            var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, typeof(IKeyGenerator), Type.EmptyTypes);
            var iLGenerator = methodBuilder.GetILGenerator();
            var localBuilder = ExpressionGenerator.EmitStringExpression(iLGenerator, attribute.Key!, descriptors);
            if (localBuilder == null)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                //new SimpleKeyGenerator.StringKeyGenerator
                var keyGeneratorConstructor = typeof(SimpleKeyGenerator.StringKeyGenerator).GetConstructors()[0];
                iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
                iLGenerator.Emit(OpCodes.Ldstr, "null");
                iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private void EmitConditionGenerator(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheBaseAttribute attribute, IList<FieldBuilderDescriptor> descriptors)
        {
            if (string.IsNullOrWhiteSpace(attribute.Condition))
            {
                iLGenerator.Emit(OpCodes.Ldnull);
                return;
            }
            var methodBuilder = DefineGetConditionGeneratorMethod(typeBuilder, index, attribute, descriptors);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, methodBuilder);
        }

        private MethodBuilder DefineGetConditionGeneratorMethod(TypeBuilder typeBuilder, int index, CacheBaseAttribute attribute, IList<FieldBuilderDescriptor> descriptors)
        {
            //create method
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            string methodName = "Get" + attribute.GetType().Name.Replace("Attribute", "") + "ConditionGenerator_" + index;
            var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, typeof(IPredicateGenerator), Type.EmptyTypes);
            var iLGenerator = methodBuilder.GetILGenerator();
            var predicateGeneratorConstructor = typeof(PredicateGenerator).GetConstructors()[0];
            var localBuilder = ExpressionGenerator.EmitBooleanExpression(iLGenerator, attribute.Condition!, descriptors);
            if (localBuilder == null)
            {
                iLGenerator.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
            }
            iLGenerator.Emit(OpCodes.Newobj, predicateGeneratorConstructor);
            iLGenerator.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private void EmitSimpleKeyGenerator(ILGenerator iLGenerator, IList<FieldBuilderDescriptor> descriptors)
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
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.StringKeyGenerator).GetConstructors()[0];
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Ldstr, "null");
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                else if (parameterType.IsNullableType() && parameterType.GenericTypeArguments[0].IsPrimitive)
                {
                    //new SimpleKeyGenerator.NullableToStringKeyGenerator<T>
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.NullableToStringKeyGenerator<>).MakeGenericType(parameterType.GenericTypeArguments[0]).GetConstructors()[0];
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Ldstr, "null");
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                else if (parameterType.IsPrimitive || parameterType.IsValueType)
                {
                    //new SimpleKeyGenerator.StructToStringKeyGenerator<T>
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.StructToStringKeyGenerator<>).MakeGenericType(parameterType).GetConstructors()[0];
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
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.JsonKeyGenerator<>).MakeGenericType(parameterType).GetConstructors()[0];
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, descriptor.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Ldstr, "null");
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
                descriptor.EmitValue(iLGenerator, true);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            //new SimpleKeyGenerator.JsonKeyGenerator<>
            var jsonKeyGeneratorConstructor = typeof(SimpleKeyGenerator.JsonKeyGenerator<>).MakeGenericType(typeof(object[])).GetConstructors()[0];
            iLGenerator.Emit(OpCodes.Ldstr, "null");
            iLGenerator.Emit(OpCodes.Newobj, jsonKeyGeneratorConstructor);
        }

        //protected static void AddExplicitAutoProperty<T>(TypeBuilder typeBuilder, T instance) where T : ISpringCachingRequirement
        //{
        //    foreach (var property in typeof(T).GetProperties())
        //    {
        //        MethodAttributes methodAttributes =
        //            MethodAttributes.Private
        //            | MethodAttributes.SpecialName
        //            | MethodAttributes.HideBySig
        //            | MethodAttributes.NewSlot
        //            | MethodAttributes.Virtual
        //            | MethodAttributes.Final;

        //        //string prefix = property.DeclaringType.FullName + ".";
        //        string prefix = property.DeclaringType!.GetFullName() + ".";
        //        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(prefix + property.Name, PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);
        //        if (property.CanRead)
        //        {
        //            MethodBuilder propertyGet = typeBuilder.DefineMethod(prefix + "get_" + property.Name, methodAttributes, property.PropertyType, Type.EmptyTypes);
        //            //propertyGet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
        //            ILGenerator iLGenerator = propertyGet.GetILGenerator();
        //            var value = property.GetValue(instance);
        //            iLGenerator.EmitObjectValue(value);
        //            iLGenerator.Emit(OpCodes.Ret);
        //            typeBuilder.DefineMethodOverride(propertyGet, property.GetMethod!);
        //            propertyBuilder.SetGetMethod(propertyGet);
        //        }

        //        //if (property.CanWrite)
        //        //{
        //        //    MethodBuilder propertySet = typeBuilder.DefineMethod(prefix + "set_" + property.Name, methodAttributes, typeof(void), new Type[] { property.PropertyType });
        //        //    //propertySet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
        //        //    propertySet.DefineParameter(1, ParameterAttributes.None, "value");
        //        //    ILGenerator iLGenerator = propertySet.GetILGenerator();
        //        //    iLGenerator.Emit(OpCodes.Ldarg_0);
        //        //    iLGenerator.Emit(OpCodes.Ldfld, instanceFieldBuilder);
        //        //    iLGenerator.Emit(OpCodes.Ldarg_1);
        //        //    iLGenerator.Emit(OpCodes.Callvirt, property.SetMethod!);
        //        //    iLGenerator.Emit(OpCodes.Ret);
        //        //    typeBuilder.DefineMethodOverride(propertySet, property.SetMethod!);
        //        //    propertyBuilder.SetSetMethod(propertySet);
        //        //}

        //        propertyBuilder.CopyCustomAttributes(property);

        //    }
        //}


    }

}
