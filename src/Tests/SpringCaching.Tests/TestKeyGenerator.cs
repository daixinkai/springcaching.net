using SpringCaching.Infrastructure;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Tests
{
    public class TestKeyGenerator : IKeyGenerator
    {
        public string GetKey(string expression, ISpringCachingRequirement requirement)
        {
            throw new NotImplementedException();
        }
    }
}
