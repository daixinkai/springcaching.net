#if NET45 || NETSTANDARD2_0
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Serialization
{
    public class NewtonsoftJsonCacheSerializer : IJsonCacheSerializer
    {
        public static readonly NewtonsoftJsonCacheSerializer JsonCacheSerializer = new NewtonsoftJsonCacheSerializer();
        public JsonSerializerSettings JsonSerializerSettings { get; } = CreateDefaultSerializerSettings();

        public byte[] SerializeObject(object? value)
        {
            string json = JsonConvert.SerializeObject(value, JsonSerializerSettings);
            return Encoding.UTF8.GetBytes(json);
        }

        public TResult? DeserializeObject<TResult>(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return default;
            }
            string json = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject<TResult>(json, JsonSerializerSettings);
        }

        public static JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
#if !NETFX_CORE // DataContractResolver is not supported in portable library
                //ContractResolver = _defaultContractResolver,
#endif

                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                TypeNameHandling = TypeNameHandling.None
            };
        }

    }
}

#endif