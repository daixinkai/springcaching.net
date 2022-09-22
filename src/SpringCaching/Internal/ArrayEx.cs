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
    }
}
