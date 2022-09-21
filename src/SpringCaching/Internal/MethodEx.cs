using SpringCaching.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Internal
{
    internal static class MethodEx
    {
        public static MethodInfo? GetMethod(Type type, string name, BindingFlags bindingAttr, Type[] types)
        {
#if NET6_0_OR_GREATER
            return type.GetMethod(name, bindingAttr, types);
#else
            return type.GetMethods(bindingAttr)
                .Where(s => s.Name == name && ParameterEquals(s.GetParameters(), types))
                .FirstOrDefault();
#endif
        }
#if !NET6_0_OR_GREATER

        private static bool ParameterEquals(ParameterInfo[] array1, Type[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i].ParameterType != array2[i])
                {
                    return false;
                }
            }
            return true;
        }

#endif
    }
}
