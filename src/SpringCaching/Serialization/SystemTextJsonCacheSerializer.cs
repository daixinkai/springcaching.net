#if !NET45 && !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SpringCaching.Serialization;

namespace SpringCaching.Serialization
{
    public class SystemTextJsonCacheSerializer : IJsonCacheSerializer
    {
        public static readonly SystemTextJsonCacheSerializer JsonCacheSerializer = new SystemTextJsonCacheSerializer();
        public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public byte[] SerializeObject(object? value)
        {
            //if (value == null)
            //{
            //    return JsonSerializer.SerializeToUtf8Bytes(value, typeof(object), JsonSerializerOptions);
            //}
            //return JsonSerializer.SerializeToUtf8Bytes(value, value!.GetType(), JsonSerializerOptions);
            return JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);
        }

        public TResult? DeserializeObject<TResult>(byte[] value)
        {
            return JsonSerializer.Deserialize<TResult>(value, JsonSerializerOptions);
        }


    }
}

#endif