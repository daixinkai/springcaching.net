using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using SpringCaching.Reflection;
using SpringCaching.Requirement;
using System.Runtime.InteropServices;

namespace SpringCaching
{
    internal static partial class ReflectionExtensions
    {
        public static bool IsTaskMethod(this MethodInfo method)
        {
            return method.ReturnType == typeof(Task) || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
        }

        public static void SetCustomAttribute<TAttribute>(this MethodBuilder methodBuilder, Expression<Func<TAttribute>> expression) where TAttribute : Attribute
        {
            methodBuilder.SetCustomAttribute(GetCustomAttributeBuilder(expression));
        }

        public static void CopyCustomAttributes(this PropertyBuilder propertyBuilder, PropertyInfo property)
        {
            var datas = CustomAttributeData.GetCustomAttributes(property);
            foreach (var data in datas)
            {
                CustomAttributeBuilder customAttributeBuilder = GetCustomAttributeBuilder(data);
                propertyBuilder.SetCustomAttribute(customAttributeBuilder);
            }
        }

        public static void CopyCustomAttributes(this MethodBuilder methodBuilder, MethodInfo method)
        {
            var datas = CustomAttributeData.GetCustomAttributes(method);
            foreach (var data in datas)
            {
                CustomAttributeBuilder customAttributeBuilder = GetCustomAttributeBuilder(data);
                methodBuilder.SetCustomAttribute(customAttributeBuilder);
            }
        }

        public static void CopyCustomAttributes(this TypeBuilder typeBuilder, Type type, params Type[] excludeAttributeTypes)
        {
            var datas = CustomAttributeData.GetCustomAttributes(type);
            foreach (var data in datas)
            {
                if (excludeAttributeTypes != null && excludeAttributeTypes.Contains(data.AttributeType))
                {
                    continue;
                }
                CustomAttributeBuilder customAttributeBuilder = GetCustomAttributeBuilder(data);
                typeBuilder.SetCustomAttribute(customAttributeBuilder);
            }
        }

        public static string? GetFullName(this Type type)
        {
            try
            {
                if (!type.IsGenericType)
                {
                    return type.FullName;
                }
                Dictionary<Type, string> cache = new Dictionary<Type, string>();
                return GetFullName(type, cache);
            }
            catch //(Exception ex)
            {
                return type.FullName;
            }
        }

        public static void DefineExplicitAutoPropertyFromField(this TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder? fieldBuilder)
        {
            if (property.DeclaringType == null || !property.DeclaringType.IsInterface)
            {
                throw new ArgumentException(nameof(property));
            }

            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot
                | MethodAttributes.Virtual
                | MethodAttributes.Final;

            string prefix = property.DeclaringType.GetFullName() + ".";
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(prefix + property.Name, PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);
            if (property.CanRead)
            {
                MethodBuilder propertyGet = typeBuilder.DefineMethod(prefix + "get_" + property.Name, methodAttributes, property.PropertyType, Type.EmptyTypes);
                //propertyGet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
                ILGenerator iLGenerator = propertyGet.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                if (fieldBuilder != null)
                {
                    iLGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                }
                iLGenerator.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(propertyGet, property.GetMethod!);
                propertyBuilder.SetGetMethod(propertyGet);
            }

            if (property.CanWrite)
            {
                MethodBuilder propertySet = typeBuilder.DefineMethod(prefix + "set_" + property.Name, methodAttributes, typeof(void), new Type[] { property.PropertyType });
                //propertySet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
                propertySet.DefineParameter(1, ParameterAttributes.None, "value");
                ILGenerator iLGenerator = propertySet.GetILGenerator();
                if (fieldBuilder != null)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                    iLGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                }
                iLGenerator.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(propertySet, property.SetMethod!);
                propertyBuilder.SetSetMethod(propertySet);
            }

            propertyBuilder.CopyCustomAttributes(property);

        }

        public static void EmitObjectValue(this ILGenerator iLGenerator, object? value)
        {
            if (value == null)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else if (value is string stringValue)
            {
                iLGenerator.Emit(OpCodes.Ldstr, stringValue);
            }
            else if (value is bool boolValue)
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
            else if (value is int int32Value)
            {
                iLGenerator.EmitInt32Value(int32Value);
            }
            else if (value is Enum enumValue)
            {
                iLGenerator.EmitInt32Value(enumValue.GetHashCode());
            }
        }

        public static void EmitSetProperty(this ILGenerator iLGenerator, PropertyInfo property, object? value, bool dup)
        {
            if (dup)
            {
                iLGenerator.Emit(OpCodes.Dup);
            }
            iLGenerator.EmitObjectValue(value);
            iLGenerator.Emit(OpCodes.Callvirt, property.SetMethod!);
            if (dup)
            {
                iLGenerator.Emit(OpCodes.Nop);
            }
        }

        public static void EmitInt32Value(this ILGenerator iLGenerator, int value)
        {
            switch (value)
            {
                case -1:
                    iLGenerator.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    iLGenerator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    iLGenerator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    iLGenerator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    iLGenerator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    iLGenerator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    iLGenerator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    iLGenerator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    iLGenerator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    iLGenerator.Emit(OpCodes.Ldc_I4_S, value);
                    break;
            }
        }

        public static LocalBuilder? EmitNullablePropertyValue(this ILGenerator iLGenerator, PropertyInfo property)
            => EmitNullableMethod(iLGenerator, property!.GetMethod!);

        public static void EmitNullablePropertyValue(this ILGenerator iLGenerator, PropertyInfo property, ref LocalBuilder? localBuilder)
             => EmitNullableMethod(iLGenerator, property!.GetMethod!, ref localBuilder);

        public static LocalBuilder? EmitNullableMethod(this ILGenerator iLGenerator, MethodInfo method)
        {
            LocalBuilder? localBuilder = null;
            EmitNullableMethod(iLGenerator, method, ref localBuilder);
            return localBuilder;
        }
        public static void EmitNullableMethod(this ILGenerator iLGenerator, MethodInfo method, ref LocalBuilder? localBuilder)
        {
            if (localBuilder == null)
            {
                localBuilder = iLGenerator.DeclareLocal(method.DeclaringType!);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            }
            iLGenerator.Emit(OpCodes.Ldloca, localBuilder);
            iLGenerator.Emit(OpCodes.Call, method);
        }

        public static void OverrideProperty(this TypeBuilder typeBuilder, PropertyInfo property, Action<ILGenerator>? getterInvoker, Action<ILGenerator>? setterInvoker)
        {
            MethodAttributes methodAttributes =
                 MethodAttributes.Public
                 | MethodAttributes.SpecialName
                 | MethodAttributes.HideBySig
                 | MethodAttributes.Virtual;
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, Type.EmptyTypes);
            if (property.CanRead)
            {
                MethodBuilder propertyGet = typeBuilder.DefineMethod("get_" + property.Name, methodAttributes, property.PropertyType, Type.EmptyTypes);
                //propertyGet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
                ILGenerator iLGenerator = propertyGet.GetILGenerator();
                getterInvoker?.Invoke(iLGenerator);
                typeBuilder.DefineMethodOverride(propertyGet, property.GetMethod!);
                propertyBuilder.SetGetMethod(propertyGet);
            }

            if (property.CanWrite)
            {
                MethodBuilder propertySet = typeBuilder.DefineMethod("set_" + property.Name, methodAttributes, typeof(void), new Type[] { property.PropertyType });
                //propertySet.SetCustomAttribute(() => new CompilerGeneratedAttribute());
                propertySet.DefineParameter(1, ParameterAttributes.None, "value");
                ILGenerator iLGenerator = propertySet.GetILGenerator();
                setterInvoker?.Invoke(iLGenerator);
                iLGenerator.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(propertySet, property.SetMethod!);
                propertyBuilder.SetSetMethod(propertySet);
            }

            propertyBuilder.CopyCustomAttributes(property);

        }

        private static string? GetFullName(Type type, Dictionary<Type, string> cache)
        {
            if (!type.IsGenericType)
            {
                return type.FullName;
            }
            if (cache.TryGetValue(type, out var cacheFullName))
            {
                return cacheFullName;
            }
            string genericArgumentName = string.Join(",", type.GetGenericArguments().Select(s => GetFullName(s, cache)));
            string fullName = type.GetGenericTypeDefinition().FullName!.Split(new char[] { '`' })[0];
            fullName = fullName + "<" + genericArgumentName + ">";
            cache.Add(type, fullName);
            return fullName;
        }

        /// <summary>
        /// 生成调用父类方法  base.xxxxx(xxx)
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MethodBuilder DefineBaseTypeMethod(this TypeBuilder typeBuilder, MethodInfo method)
        {

            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;

            var parameters = method.GetParameters();
            Type[] parameterTypes = parameters.Select(s => s.ParameterType).ToArray();
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name + "<>n__", methodAttributes, CallingConventions.Standard, method.ReturnType, parameterTypes);

            #region parameterName

            for (int i = 0; i < parameters.Length; i++)
            {
                methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, parameters[i].Name);
            }

            #endregion

            methodBuilder.SetCustomAttribute(() => new CompilerGeneratedAttribute());

            //methodBuilder.CopyCustomAttributes(method);

            ILGenerator iLGenerator = methodBuilder.GetILGenerator();


            iLGenerator.Emit(OpCodes.Ldarg_0); // this
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                iLGenerator.Emit(OpCodes.Ldarg_S, i + 1);
            }
            iLGenerator.Emit(OpCodes.Call, method);
            iLGenerator.Emit(OpCodes.Ret);

            return methodBuilder;

        }

        private static CustomAttributeBuilder GetCustomAttributeBuilder(CustomAttributeData data)
        {
            List<CustomAttributeNamedArgument> propertyArguments = data.NamedArguments.Where(s => !s.IsField).ToList();
            List<CustomAttributeNamedArgument> fieldArguments = data.NamedArguments.Where(s => s.IsField).ToList();
            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                data.Constructor,
                data.ConstructorArguments.Select(GetArgumentValue).ToArray(),
                propertyArguments.Select(s => (PropertyInfo)s.MemberInfo).ToArray(),
                propertyArguments.Select(s => s.TypedValue.Value).ToArray(),
                fieldArguments.Select(s => (FieldInfo)s.MemberInfo).ToArray(),
                fieldArguments.Select(s => s.TypedValue.Value).ToArray());
            return customAttributeBuilder;
        }

        private static CustomAttributeBuilder GetCustomAttributeBuilder<TAttribute>(Expression<Func<TAttribute>> expression) where TAttribute : Attribute
        {
            if (expression.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException(nameof(expression));
            }
            if (expression.Body.NodeType == ExpressionType.New)
            {
                NewExpression? newExpression = expression.Body as NewExpression;
                return new CustomAttributeBuilder(
                    newExpression!.Constructor!,
                    newExpression.Arguments.OfType<ConstantExpression>().Select(s => s.Value).ToArray()
                );
            }
            else if (expression.Body.NodeType == ExpressionType.MemberInit)
            {
                MemberInitExpression? memberInitExpression = expression.Body as MemberInitExpression;
                var fields = memberInitExpression!.Bindings.OfType<MemberAssignment>().Where(s => s.Member.MemberType == MemberTypes.Field).ToList();
                var properties = memberInitExpression.Bindings.OfType<MemberAssignment>().Where(s => s.Member.MemberType == MemberTypes.Property).ToList();
                return new CustomAttributeBuilder(
                    memberInitExpression!.NewExpression.Constructor!,
                    memberInitExpression.NewExpression.Arguments.OfType<ConstantExpression>().Select(s => s.Value).ToArray(),
                    properties.Select(s => s.Member).OfType<PropertyInfo>().ToArray(),
                    properties.Select(s => s.Expression).OfType<ConstantExpression>().Select(s => s.Value).ToArray(),
                    fields.Select(s => s.Member).OfType<FieldInfo>().ToArray(),
                    fields.Select(s => s.Expression).OfType<ConstantExpression>().Select(s => s.Value).ToArray()
                 );
            }
            throw new ArgumentException(nameof(expression));
        }

        private static object GetArgumentValue(CustomAttributeTypedArgument argument)
        {
            if (argument.ArgumentType.IsArray)
            {
                var values = argument.Value as System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>;
                return values!.Select(GetArgumentValue).ToArray();
            }
            return argument.Value!;
        }

    }
}
