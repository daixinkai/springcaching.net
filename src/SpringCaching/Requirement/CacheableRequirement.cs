using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public class CacheableRequirement : ICacheableRequirement
    {
        public CacheableRequirement(string value, IKeyGenerator? keyGenerator)
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

        public ExpirationPolicy ExpirationPolicy { get; set; }

        public ExpirationUnit ExpirationUnit { get; set; }

        public int ExpirationValue { get; set; }

        public IKeyGenerator? KeyGenerator { get; }

    }
}
