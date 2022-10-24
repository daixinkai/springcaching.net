using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Internal
{
    internal static class ArrayEx
    {
        public static T[] Empty<T>()
        {
#if NET45
            return new T[0];
#else
            return Array.Empty<T>();
#endif
        }

        public static void ForEach<T>(IList<T> list, Func<T, T?, bool> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var current = list[i];
                var next = i < list.Count - 1 ? list[i + 1] : default;
                if (action(current, next))
                {
                    list.RemoveAt(i + 1);
                    i--;
                }
            }
        }

        public static void ForEach<T>(IList<T> list, Action<T, T?> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var current = list[i];
                var next = i < list.Count - 1 ? list[i + 1] : default;
                action(current, next);
            }
        }

    }
}
