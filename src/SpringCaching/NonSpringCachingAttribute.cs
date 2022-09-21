using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class NonSpringCachingAttribute : Attribute
    {
    }
}
