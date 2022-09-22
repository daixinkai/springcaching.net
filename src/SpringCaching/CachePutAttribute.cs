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
        /// <inheritdoc />
        public string? Unless { get; set; }
    }
}
