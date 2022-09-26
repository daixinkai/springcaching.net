using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching
{
    /// <summary>
    /// <para>Annotation indicating that a method (or all methods on a class) triggers a cache put operation.</para>
    /// <para>In contrast to the @Cacheable annotation, this annotation does not cause the advised method to be skipped. Rather, it always causes the method to be invoked and its result to be stored in the associated cache. Note that Java8's Optional return types are automatically handled and its content is stored in the cache if present.</para>
    /// <para>This annotation may be used as a meta-annotation to create custom composed annotations with attribute overrides.</para>
    /// </summary>
    public class CachePutAttribute : CacheBaseAttribute
    {
        public CachePutAttribute(string value) : base(value)
        {
        }
        /// <summary>
        /// ExpirationPolicy
        /// </summary>
        public ExpirationPolicy ExpirationPolicy { get; set; }
        /// <summary>
        /// ExpirationUnit
        /// </summary>
        public ExpirationUnit ExpirationUnit { get; set; }
        /// <summary>
        /// ExpirationValue
        /// </summary>
        public int ExpirationValue { get; set; }
        /// <summary>
        /// if result is null, don't cache it
        /// </summary>
        public bool UnlessNull { get; set; }
    }
}
