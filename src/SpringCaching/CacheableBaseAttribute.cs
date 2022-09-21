using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SpringCaching
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class CacheableBaseAttribute : Attribute
    {
        public CacheableBaseAttribute(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        /// <inheritdoc />
        public string Value { get; }
        /// <inheritdoc />
        public string? Key { get; set; }
        /// <inheritdoc />
        public string? Condition { get; set; }

    }
}
