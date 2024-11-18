using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Tests
{
    [SpringCaching]
    public class TestUserService
    {
        [Cacheable("users",
            Key = "#id",
            Condition = "#id>0",
            //ConditionGenerator = typeof(TestPredicateGenerator),
            ResultCondition ="#result!=null",
            ExpirationPolicy = ExpirationPolicy.Absolute,
            ExpirationUnit = ExpirationUnit.Minute,
            ExpirationValue = 600)]
        public virtual Task<UserResultDto> GetUserAsync(int id)
        {
            return Task.FromResult(new UserResultDto
            {
                Id = id
            });
        }

        [CacheEvict("users", Key = "#user.Id", Condition = "#user!=null&&#user.Id>0")]
        public virtual Task UpdateUserAsync(UserResultDto user)
        {
            return Task.FromResult(0);
        }

        [CacheEvict("users", AllEntries = true)]
        public virtual Task ClearCache()
        {
            return Task.FromResult(0);
        }

    }
}
