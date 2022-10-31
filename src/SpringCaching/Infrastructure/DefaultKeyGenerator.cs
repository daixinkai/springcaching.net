using SpringCaching.Parsing;
using SpringCaching.Requirement;
using SpringCaching.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    internal class DefaultKeyGenerator : IKeyGenerator
    {
        public static readonly DefaultKeyGenerator Instance = new DefaultKeyGenerator();
        public string? GetKey(string? expression, ISpringCachingRequirement requirement)
        {
            var arguments = requirement.Arguments;
            if (arguments == null)
            {
                return "null";
            }
#if NET45 || NETSTANDARD2_0
            string json = Encoding.UTF8.GetString(NewtonsoftJsonCacheSerializer.JsonCacheSerializer.SerializeObject(arguments));
#else
                string json = Encoding.UTF8.GetString(SystemTextJsonCacheSerializer.JsonCacheSerializer.SerializeObject(arguments));
#endif
            return json.Replace(":", "-");
        }
    }
}
