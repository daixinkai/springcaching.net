using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SpringCachingAttribute : Attribute
    {
    }
}
