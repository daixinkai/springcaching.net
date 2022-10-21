using SpringCaching.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Tests
{
    [SpringCaching]
    public class TestService : ITestService//, ISpringCachingProxy
    {

        public string ServiceId { get; set; }

        public Type ServiceType { get; set; }

        public ICacheProvider CacheProvider => throw new NotImplementedException();

        public SpringCachingOptions Options => throw new NotImplementedException();

        //[Cacheable("getAllNames", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        //[CachePut("getAllNames_CachePut", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetAllNames()
        {
            Random random = new Random();
            return GetAllNameTests(new int[random.Next(1, 10)].Select(s => random.Next(1, 10000)).ToArray(), null, 1);
        }

        //[Cacheable("getAllNameTests", Key = "#ids", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetAllNameTests(int[] ids, string[] names, int id)
        {
            Random random = new Random();
            return Task.FromResult(new int[random.Next(1, 10)].Select(s => random.Next(1, 10000).ToString()).ToList());
        }

        //[Cacheable("getNames", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(int id)
        {
            int count = GetCount(id);
            return Task.FromResult(RandomNames(count));
        }

        //[Cacheable("getNames", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(int? id)
        {
            int count = GetCount(id.GetValueOrDefault());
            return Task.FromResult(RandomNames(count));
        }

        [Cacheable("getNames_Param",
            //Key = "#param.Id + #param.Count + #param.Name", 
            //Key = "#param?.Param?.Param?.Id + #param.Count + #param.Name",
            //Key = "#param?.Id + #param.Count + #param.Name",
            //Key = "#param?.Param?.Param?.Param?.Param?.Name",
            //Key = "#param?.Param?.Name+'-'+#param.Name",
            //Key = "#param?.Param?.Count",
            //Condition = "#param?.Id>0&&#param.Count!=0",
            //Condition = "#param.Id>0",
            //Condition = "#param.Param?.Name!='asd'",
            //Condition = "#param.Id.HasValue",
            //Condition = "#param.Id>0&&#param.Count!=0||#param.Id>0",
            Condition = "#param.Count>=0&&#param.Name!=null",
            //Condition = "#param.Id!=null&&#param.Name!=null",
            //Condition = "#param.Id.HasValue&&#param.Id.Value>0&&#param?.Param?.Name!=null&&#param.Count>0",
            //Condition = "#param.Id.HasValue&&#param.Id.Value>0&&#param?.Param?.Name!=null&&#param.Count>0",
            //Condition = "#param?.Param?.Name!=null",
            //Condition = "!#param.Id.HasValue",
            ExpirationPolicy = ExpirationPolicy.Absolute,
            ExpirationUnit = ExpirationUnit.Minute,
            ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(TestServiceParam param)
        {
            return Task.FromResult(new List<string>() { param.Id.ToString(), param.Name + "_" + Guid.NewGuid().ToString("N") });
        }


        //[Cacheable("getNames_id", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(string id)
        {
            int count = GetCount(0);
            return Task.FromResult(RandomNames(count));
        }

        //[Cacheable("getNamesTest", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        protected internal virtual Task<List<string>> GetNamesTest(int id)
        {
            return Task.FromResult(RandomNames(id));
        }

        //[Cacheable("getCount", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        protected internal virtual int GetCount(int id)
        {
            return new Random().Next(1, 1000);
        }

        //[CacheEvict("getNames", Condition = "#id!=0")]
        protected internal virtual Task SetNamesAsync(int id, List<string> names)
        {
            return Task.FromResult(new List<string>());
        }

        //[CacheEvict("getNames", AllEntries = true)]
        //[CacheEvict("*", AllEntries = true)]
        public virtual Task UpdateNames()
        {
            return Task.FromResult(0);
        }

        private List<string> RandomNames(int count)
        {
            Random random = new Random();
            return new int[random.Next(1, count)].Select(s => random.Next(1, 10000).ToString()).ToList();
        }

        Task<List<string>> ITestService.GetNames(int id)
        {
            return GetNames(id);
        }
    }
}
