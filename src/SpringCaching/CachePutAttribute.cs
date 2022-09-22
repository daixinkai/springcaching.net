using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching
{
    //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CachePutAttribute : CacheableBaseAttribute
    {
        public CachePutAttribute(string value) : base(value)
        {
        }
        public ExpirationPolicy ExpirationPolicy { get; set; }
        public ExpirationUnit ExpirationUnit { get; set; }
        public int ExpirationValue { get; set; }
        /// <summary>
        /// if result is null, don't cache it
        /// </summary>
        public bool UnlessNull { get; set; }
    }
}
