#if NET45 || NETSTANDARD2_0
using Newtonsoft.Json.Linq;
using SpringCaching.Formatting;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure.Parser
{
    internal class DefaultJsonExpressionParser
    {
        public const string ExpressionPrefix = "#{";
        public const string ExpressionSuffix = "}";

#if NET45 || NETSTANDARD2_0
        private static readonly JsonSerializerSettings s_jsonSerializerSettings = NewtonsoftJsonCacheSerializer.CreateDefaultSerializerSettings();
        protected static JToken? GetJsonElement(JObject jObject, List<string> paramList)
        {
            JToken? jToken = jObject;
            for (int i = 0; i < paramList.Count; i++)
            {
                jToken = jToken![paramList[i]];
                if (jToken == null || jToken.Type == JTokenType.Undefined || jToken.Type == JTokenType.Null)
                {
                    return null;
                }
            }
            return jToken;
        }
        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, s_jsonSerializerSettings);
        }
        public static JObject SerializeToDocument(object value)
        {
            return JObject.FromObject(value);
        }
        public static string? GetStringValue(JToken jToken)
        {
            return jToken.ToString();
            //switch (jToken.Type)
            //{
            //    case JTokenType.Object:
            //    case JTokenType.Array:
            //        return jToken.ToString();
            //    case JTokenType.String:
            //        return jToken.Value<string>();
            //    case JTokenType.:
            //        return jsonElement.GetInt64().ToString();
            //    case JTokenType.Boolean:
            //        return jsonElement.GetBoolean().ToString();
            //    default:
            //        return null;
            //}
        }
#else
        private static readonly JsonSerializerOptions s_jsonSerializerSettings = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        protected static JsonElement? GetJsonElement(JsonDocument jObject, List<string> paramList)
        {
            JsonElement jToken = jObject.RootElement;
            for (int i = 0; i < paramList.Count; i++)
            {
                jToken = SafeGetProperty(jToken, paramList[i]);
                if (jToken.ValueKind == JsonValueKind.Undefined || jToken.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }
            }
            return jToken;
        }

        private static JsonElement SafeGetProperty(JsonElement jsonElement, string propertyName)
        {
            jsonElement.TryGetProperty(propertyName, out var value);
            return value;
        }

        public static string SerializeObject(object value)
        {
            return JsonSerializer.Serialize(value, value.GetType(), s_jsonSerializerSettings);
        }
        public static JsonDocument SerializeToDocument(object value)
        {
            return JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(value, value.GetType()));
        }

        public static string? GetStringValue(JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    return jsonElement.GetRawText().Trim('\\');
                case JsonValueKind.Array:
                    return jsonElement.GetRawText().Trim('\\');
                case JsonValueKind.String:
                    return jsonElement.GetString();
                case JsonValueKind.Number:
                    return jsonElement.GetInt64().ToString();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean().ToString();
                default:
                    return null;
            }
        }

#endif



    }
}
