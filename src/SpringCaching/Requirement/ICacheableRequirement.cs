using System;
using System.Collections.Generic;
using System.Text;
using SpringCaching.Infrastructure;

namespace SpringCaching.Requirement
{
    public interface ICacheableRequirement : ICacheRequirement
    {
        /// <summary>
        /// if result is null, don't cache it
        /// </summary>
        public bool UnlessNull { get; set; }

        ExpirationPolicy ExpirationPolicy { get; }
        ExpirationUnit ExpirationUnit { get; }
        int ExpirationValue { get; }

    }
}
