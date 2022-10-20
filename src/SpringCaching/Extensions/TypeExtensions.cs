using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpringCaching
{
    internal static class TypeExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        public static bool IsValueTypeEx(this Type type)
        {
            return type.IsValueType && !type.IsNullableType();
        }

        public static ConstructorInfo GetConstructorEx(this Type type)
        {
            return type.GetConstructors()[0];
        }

        public static ConstructorInfo GetConstructorEx(this Type type, Type[] types)
        {
            return type.GetConstructors().Where(s => Equals(s.GetParameters(), types)).FirstOrDefault();
        }

        private static bool Equals(ParameterInfo[] parameters, Type[] types)
        {
            if (types.Length == 0)
            {
                return parameters.Length == 0;
            }
            if (parameters.Length != types.Length)
            {
                return false;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != types[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
