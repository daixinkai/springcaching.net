using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SpringCaching
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class CacheBaseAttribute : Attribute
    {
        public CacheBaseAttribute(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        /// <inheritdoc />
        public string Value { get; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression for computing the key dynamically.
        /// Default is "", meaning all method parameters are considered as a key, unless a custom keyGenerator has been configured.
        /// </summary>
        public string? Key { get; set; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression used for making the method caching conditional.
        /// Default is "", meaning the method result is always cached.
        /// </summary>
        [Obsolete]
        public string? Condition { get; set; }

    }
}
