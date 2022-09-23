using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Tests
{
    [SpringCaching]
    public class TestService : ITestService
    {

        public string ServiceId { get; set; }

        public Type ServiceType { get; set; }


        [Cacheable("getAllNames", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        [CachePut("getAllNames_CachePut", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetAllNames()
        {
            Random random = new Random();
            //return Task.FromResult(new string[random.Next(1, 10)].Select(s => random.Next(1, 10000).ToString()).ToList());
            return GetAllNameTests(new int[random.Next(1, 10)].Select(s => random.Next(1, 10000)).ToArray(), null, 1);
            //return Task.FromResult(default(List<string>));
        }

        [Cacheable("getAllNameTests", Key = "#ids", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetAllNameTests(int[] ids, string[] names, int id)
        {
            Random random = new Random();
            return Task.FromResult(new int[random.Next(1, 10)].Select(s => random.Next(1, 10000).ToString()).ToList());
        }

        [Cacheable("getNames", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(int id)
        {
            int count = GetCount(id);
            return Task.FromResult(new List<string>() { count.ToString() });
        }

        [Cacheable("getNames_Param", Key = "#param.Id", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(TestServiceParam param)
        {
            return Task.FromResult(new List<string>() { param.Id.ToString(), param.Name });
        }


        [Cacheable("getNames_id", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(string id)
        {
            int count = GetCount(0);
            return Task.FromResult(new List<string>() { count.ToString() });
        }

        [Cacheable("getNamesTest", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        protected internal virtual Task<List<string>> GetNamesTest(int id)
        {
            return Task.FromResult(new List<string>());
        }

        [Cacheable("getCount", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        protected internal virtual int GetCount(int id)
        {
            return new Random().Next(1, 1000);
        }

        [CacheEvict("getNames")]
        protected internal virtual Task SetNamesAsync(int id, List<string> names)
        {
            return Task.FromResult(new List<string>());
        }

        [CacheEvict("getNames")]
        protected internal virtual void SetNames(int id, List<string> names)
        {
        }

        Task<List<string>> ITestService.GetNames(int id)
        {
            return GetNames(id);
        }
    }
}
