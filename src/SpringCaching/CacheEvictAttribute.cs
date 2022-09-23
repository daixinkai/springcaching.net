using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using SpringCaching.Requirement;

namespace SpringCaching
{
    //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
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
