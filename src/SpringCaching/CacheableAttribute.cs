using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SpringCaching
{
    //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CacheableAttribute : CacheableBaseAttribute
    {
        public CacheableAttribute(string value) : base(value)
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
