using System;
using System.Collections.Generic;
using System.Text;
using SpringCaching.Infrastructure;

namespace SpringCaching.Requirement
{
    public interface ICacheEvictRequirement : ICacheableRequirementBase
    {
        /// <summary>
        /// Whether all the entries inside the cache(s) are removed.
        /// By default, only the value under the associated key is removed.
        /// Note that setting this parameter to true and specifying a key is not allowed.
        /// </summary>
        bool AllEntries { get; }
        /// <summary>
        /// Whether the eviction should occur before the method is invoked.
        ///  Setting this attribute to true, causes the eviction to occur irrespective of the method outcome (i.e., whether it threw an exception or not).
        /// Defaults to false, meaning that the cache eviction operation will occur after the advised method is invoked successfully (i.e.only if the invocation did not throw an exception).
        /// </summary>
        bool BeforeInvocation { get; }

    }
}
