using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using SpringCaching.Requirement;

namespace SpringCaching
{
    /// <summary>
    /// <para>Annotation indicating that a method (or all methods on a class) triggers a cache evict operation.</para>
    /// <para>This annotation may be used as a meta-annotation to create custom composed annotations with attribute overrides.</para>
    /// </summary>
    public class CacheEvictAttribute : CacheBaseAttribute
    {
        public CacheEvictAttribute(string value) : base(value)
        {
        }
        /// <inheritdoc />
        public bool AllEntries { get; set; }
        /// <inheritdoc />
        public bool BeforeInvocation { get; set; }

    }
}
