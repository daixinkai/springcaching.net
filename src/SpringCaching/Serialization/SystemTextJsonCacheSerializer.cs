#if !NET45 && !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public byte[] SerializeObject(object? value)
            => JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);

        public TResult? DeserializeObject<TResult>(byte[] value)
            => JsonSerializer.Deserialize<TResult>(value, JsonSerializerOptions);


    }
}

#endif