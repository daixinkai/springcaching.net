using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching
{
    //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    [Obsolete("not support now")]
    public sealed class CachePutAttribute : CacheableBaseAttribute
    {
        public CachePutAttribute(string value) : base(value)
        {
        }
        /// <summary>
        /// if result is null, don't cache it
        /// </summary>
        public bool UnlessNull { get; set; }
    }
}
