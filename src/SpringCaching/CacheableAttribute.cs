using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SpringCaching
{
    //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class CacheableAttribute : CacheableBaseAttribute
    {
        public CacheableAttribute(string value) : base(value)
        {
        }
        public ExpirationPolicy ExpirationPolicy { get; set; }
        public ExpirationUnit ExpirationUnit { get; set; }
        public int ExpirationValue { get; set; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression used to veto method caching.
        /// Unlike condition, this expression is evaluated after the method has been called and can therefore refer to the result.
        /// Default is "", meaning that caching is never vetoed.
        /// </summary>
        [Obsolete("not support now")]
        public string? Unless { get; set; }
    }
}
