using SpringCaching.Infrastructure;
using SpringCaching.Proxy;
using SpringCaching.Requirement;
using SpringCaching.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.UnitTest.NET45
{
    public class CustomTestService : TestService, ISpringCachingProxy
    {
        public CustomTestService(ICacheProvider cacheProvider, SpringCachingOptions options)
        {
            _cacheProvider = cacheProvider;
            _options = options;
        }
        private class NestedCustomTestServiceProxy : ISpringCachingRequirement
        {
            public NestedCustomTestServiceProxy(CustomTestService customTestService, int id)
            {
                _customTestService = customTestService;
                _id = id;
                _idStr = id.ToString();
            }


            private CustomTestService _customTestService;
            private int _id;

            private string _idStr;

            public IDictionary<string, object> Arguments => throw new NotImplementedException();

            private Task<List<string>> GetNames__()
            {
                return _customTestService.GetNames__(_id);
            }

            IList<ICacheableRequirement> ISpringCachingRequirement.GetCacheableRequirements()
            {
                return new ICacheableRequirement[] {
                    new CacheableRequirement("getNames"){
                    KeyGenerator=new SimpleKeyGenerator.StringKeyGenerator(_idStr,"null")
                    },
                    new CacheableRequirement("getNames1"){
                     KeyGenerator=new SimpleKeyGenerator.StringKeyGenerator(_idStr,"null"),
                     Condition="",
                      ExpirationUnit= ExpirationUnit.Minute,
                       ExpirationPolicy= ExpirationPolicy.None
                    }
                };
            }

            IList<ICacheEvictRequirement> ISpringCachingRequirement.GetCacheEvictRequirements()
            {
                throw new NotImplementedException();
            }

            IList<ICachePutRequirement> ISpringCachingRequirement.GetCachePutRequirements()
            {
                throw new NotImplementedException();
            }
        }

        private ICacheProvider _cacheProvider;
        private SpringCachingOptions _options;

        ICacheProvider ISpringCachingProxy.CacheProvider => _cacheProvider;
        SpringCachingOptions ISpringCachingProxy.Options => _options;

        public override Task<List<string>> GetNames(int id)
        {
            //NestedCustomTestServiceProxy proxy = new NestedCustomTestServiceProxy(this, id);
            //Func<Task<List<string>>> invoker = new Func<Task<List<string>>>(proxy.GetNames__);
            //return SpringCachingProxyInvoker.InvokeAsync(new SpringCachingProxyContext(this, null), invoker);
            return SpringCachingProxyInvoker.InvokeAsync(new SpringCachingProxyContext(this, null), () => base.GetNames(id));
            //return SpringCachingProxyInvoker.InvokeAsync(new SpringCachingProxyContext(this, null), new Func<Task<List<string>>>(GetNames));
            //return SpringCachingProxyInvoker.InvokeAsync(new SpringCachingProxyContext(this, null), GetNames);
        }

        private Task<List<string>> GetNames__(int id)
        {
            return base.GetNames(id);
        }

    }

    internal class TestService_SpringCaching_8A8F131389F24AAC88F3B610639DF6EF_8AF2457B3F454975AB79AEBCB675874B : SpringCachingRequirementProxy, ICacheableRequirement, ICacheEvictRequirement
    {
        // Token: 0x06000004 RID: 4 RVA: 0x00002050 File Offset: 0x00000250
        public TestService_SpringCaching_8A8F131389F24AAC88F3B610639DF6EF_8AF2457B3F454975AB79AEBCB675874B(TestService testService, int _id)
        {
            this.testService = testService;
            this._id = _id;
        }

        // Token: 0x06000005 RID: 5 RVA: 0x0000207C File Offset: 0x0000027C
        public Task<List<string>> GetNames()
        {
            return this.testService.GetNames(this._id);
        }

        public string GetCalculatedKey()
        {
            IKeyGenerator keyGenerator = new SimpleKeyGenerator.JsonKeyGenerator<object[]>(new object[] { 1, "2", "3" }, "null");
            //return ToStructString(_id);
            return keyGenerator.GetKey(null, null);
        }



        // Token: 0x04000002 RID: 2
        private TestService testService;

        // Token: 0x04000003 RID: 3
        private int _id;

        string ICacheableRequirement.Value => null;

        string ICacheableRequirement.Key => "1";

        public override IDictionary<string, object> Arguments => null;

        ExpirationPolicy ICacheableRequirement.ExpirationPolicy => ExpirationPolicy.None;

        //string ICacheableRequirement.Condition => "";

        string ICacheEvictRequirement.Value => "1";

        string ICacheEvictRequirement.Key => "1";

        string ICacheEvictRequirement.Condition => "1";

        bool ICacheEvictRequirement.AllEntries => false;

        bool ICacheEvictRequirement.BeforeInvocation => true;

        ExpirationUnit ICacheableRequirement.ExpirationUnit => ExpirationUnit.Second;

        int ICacheableRequirement.ExpirationValue => 211;

        public IKeyGenerator KeyGenerator => throw new NotImplementedException();

        public string Condition => throw new NotImplementedException();

        public string Unless => throw new NotImplementedException();

        public IPredicateGenerator ConditionGenerator => throw new NotImplementedException();

        public IPredicateGenerator UnlessGenerator => throw new NotImplementedException();
    }

}
