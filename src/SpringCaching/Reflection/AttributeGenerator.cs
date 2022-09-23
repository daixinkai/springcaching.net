using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public abstract bool Build(TypeBuilder typeBuilder, Type attributeType, IList<Attribute> attributes, IList<FieldBuilderDescriptor> fieldBuilders);


        protected void SetDefaultProperty(ILGenerator iLGenerator, CacheBaseAttribute attribute, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            SetKeyGeneratorProperty(iLGenerator, attribute.Key, fieldBuilders);
            SetConditionGeneratorProperty(iLGenerator, attribute.Condition, fieldBuilders);
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
        }

        protected void SetKeyGeneratorProperty(ILGenerator iLGenerator, string? expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            if (fieldBuilders.Count == 0)
            {
                return;
            }
            iLGenerator.Emit(OpCodes.Dup);
            EmitKeyGenerator(iLGenerator, expression, fieldBuilders);
            iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheRequirementBase).GetProperty("KeyGenerator")!.SetMethod!);
            iLGenerator.Emit(OpCodes.Nop);
        }

        protected void SetConditionGeneratorProperty(ILGenerator iLGenerator, string? expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            //TODO: SetConditionGeneratorProperty
            //if (fieldBuilders.Count == 0)
            //{
            //    return;
            //}
            //iLGenerator.Emit(OpCodes.Dup);
            //EmitConditionGenerator(iLGenerator, expression, fieldBuilders);
            //iLGenerator.Emit(OpCodes.Callvirt, typeof(CacheableRequirementBase).GetProperty("ConditionGenerator")!.SetMethod!);
            //iLGenerator.Emit(OpCodes.Nop);
        }

        protected void EmitKeyGenerator(ILGenerator iLGenerator, string? expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                EmitSimpleKeyGenerator(iLGenerator, fieldBuilders);
                return;
            }
            iLGenerator.Emit(OpCodes.Ldnull);
        }

        protected void EmitConditionGenerator(ILGenerator iLGenerator, string? expression, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            //TODO : ConditionGenerator
            iLGenerator.Emit(OpCodes.Ldnull);
        }

        private void EmitSimpleKeyGenerator(ILGenerator iLGenerator, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            if (fieldBuilders.Count == 0)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
                return;
            }

            if (fieldBuilders.Count == 1)
            {
                // xx.ToString()
                var fieldBuilder = fieldBuilders[0];
                var parameterType = fieldBuilder.Parameter.ParameterType;
                if (parameterType == typeof(string))
                {
                    //new SimpleKeyGenerator.StringKeyGenerator
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.StringKeyGenerator).GetConstructors()[0];
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, fieldBuilder.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Ldstr, "null");
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                else if (parameterType.IsNullableType() && parameterType.GenericTypeArguments[0].IsPrimitive)
                {
                    //new SimpleKeyGenerator.NullableToStringKeyGenerator<T>
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.NullableToStringKeyGenerator<>).MakeGenericType(parameterType).GetConstructors()[0];
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, fieldBuilder.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Ldstr, "null");
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                else if (parameterType.IsPrimitive || parameterType.IsValueType)
                {
                    //new SimpleKeyGenerator.StructToStringKeyGenerator<T>
                    var keyGeneratorConstructor = typeof(SimpleKeyGenerator.StructToStringKeyGenerator<>).MakeGenericType(parameterType).GetConstructors()[0];
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, fieldBuilder.FieldBuilder);
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
                    iLGenerator.Emit(OpCodes.Ldfld, fieldBuilder.FieldBuilder);
                    iLGenerator.Emit(OpCodes.Ldstr, "null");
                    iLGenerator.Emit(OpCodes.Newobj, keyGeneratorConstructor);
                }
                return;
            }

            //new object[]{x,x,x,x,x,}
            iLGenerator.Emit(OpCodes.Ldc_I4, fieldBuilders.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(object));
            int index = 0;
            foreach (var fieldBuilder in fieldBuilders)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, fieldBuilder.FieldBuilder);
                //box
                if (fieldBuilder.Parameter.ParameterType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Box, fieldBuilder.Parameter.ParameterType);
                }
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            //new SimpleKeyGenerator.JsonKeyGenerator<>
            var jsonKeyGeneratorConstructor = typeof(SimpleKeyGenerator.JsonKeyGenerator<>).MakeGenericType(typeof(object[])).GetConstructors()[0];
            iLGenerator.Emit(OpCodes.Ldstr, "null");
            iLGenerator.Emit(OpCodes.Newobj, jsonKeyGeneratorConstructor);
        }

        protected static void AddExplicitAutoProperty<T>(TypeBuilder typeBuilder, T instance) where T : ISpringCachingRequirement
        {
            foreach (var property in typeof(T).GetProperties())
            {
                MethodAttributes methodAttributes =
                    MethodAttributes.Private
                    | MethodAttributes.SpecialName
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual
                    | MethodAttributes.Final;

                //string prefix = property.DeclaringType.FullName + ".";
                string prefix = property.DeclaringType!.GetFullName() + ".";
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(prefix + property.Name, PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);
                if (property.CanRead)
                {
                    MethodBuilder propertyGet = typeBuilder.DefineMethod(prefix + "get_" + property.Name, methodAttributes, property.PropertyType, Type.EmptyTypes);
                    //propertyGet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
                    ILGenerator iLGenerator = propertyGet.GetILGenerator();
                    var value = property.GetValue(instance);
                    iLGenerator.EmitObjectValue(value);
                    iLGenerator.Emit(OpCodes.Ret);
                    typeBuilder.DefineMethodOverride(propertyGet, property.GetMethod!);
                    propertyBuilder.SetGetMethod(propertyGet);
                }

                //if (property.CanWrite)
                //{
                //    MethodBuilder propertySet = typeBuilder.DefineMethod(prefix + "set_" + property.Name, methodAttributes, typeof(void), new Type[] { property.PropertyType });
                //    //propertySet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
                //    propertySet.DefineParameter(1, ParameterAttributes.None, "value");
                //    ILGenerator iLGenerator = propertySet.GetILGenerator();
                //    iLGenerator.Emit(OpCodes.Ldarg_0);
                //    iLGenerator.Emit(OpCodes.Ldfld, instanceFieldBuilder);
                //    iLGenerator.Emit(OpCodes.Ldarg_1);
                //    iLGenerator.Emit(OpCodes.Callvirt, property.SetMethod!);
                //    iLGenerator.Emit(OpCodes.Ret);
                //    typeBuilder.DefineMethodOverride(propertySet, property.SetMethod!);
                //    propertyBuilder.SetSetMethod(propertySet);
                //}

                propertyBuilder.CopyCustomAttributes(property);

            }
        }


    }

}
