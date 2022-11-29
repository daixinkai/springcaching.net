using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Tests
{
    public class TestCacheProviderFactory : ICacheProviderFactory
    {
        public ICacheProvider GetCacheProvider()
        {
            return new TestCacheProvider();
        }

        //public void ReleaseCacheProvider(ICacheProvider cacheProvider)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
