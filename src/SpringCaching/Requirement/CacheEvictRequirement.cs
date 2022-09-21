using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public class CacheEvictRequirement : ICacheEvictRequirement
    {
        public CacheEvictRequirement(string value, IKeyGenerator? keyGenerator)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            KeyGenerator = keyGenerator;
        }

        /// <inheritdoc />
        public string Value { get; }
        /// <inheritdoc />
        public string? Key { get; set; }
        /// <inheritdoc />
        public string? Condition { get; set; }
        /// <inheritdoc />
        public bool AllEntries { get; set; }
        /// <inheritdoc />
        public bool BeforeInvocation { get; set; }

        public IKeyGenerator? KeyGenerator { get; }

    }
}
