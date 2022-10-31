using SpringCaching.Serialization;
using SpringCaching.Parsing;
using SpringCaching.Requirement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public abstract class SimpleKeyGenerator : IKeyGenerator
    {
        public class StringKeyGenerator : SimpleKeyGenerator
        {
            public StringKeyGenerator(string? value)
            {
                _value = value;
            }

            private readonly string? _value;

            protected override string? GetKey()
            {
                return _value;
            }
        }

        public class StructToStringKeyGenerator<T> : SimpleKeyGenerator where T : struct
        {
            public StructToStringKeyGenerator(T value)
            {
                _value = value;
            }

            private readonly T _value;
            protected override string? GetKey()
            {
                return _value.ToString();
            }
        }

        public class NullableToStringKeyGenerator<T> : SimpleKeyGenerator where T : struct
        {
            public NullableToStringKeyGenerator(T? value)
            {
                _value = value;
            }

            private readonly T? _value;
            protected override string? GetKey()
            {
                if (!_value.HasValue)
                {
                    return null;
                }
                return _value.Value.ToString();
            }
        }

        public class JsonKeyGenerator<T> : SimpleKeyGenerator
        {
            public JsonKeyGenerator(T? value)
            {
                _value = value;
            }

            private readonly T? _value;

            public bool IncludePrefix { get; set; }

            protected override string? GetKey()
            {
                if (_value == null)
                {
                    return IncludePrefix ? GetPrefix() + "null" : null;
                }
#if NET45 || NETSTANDARD2_0
                string json = Encoding.UTF8.GetString(NewtonsoftJsonCacheSerializer.JsonCacheSerializer.SerializeObject(_value));
#else
                string json = Encoding.UTF8.GetString(SystemTextJsonCacheSerializer.JsonCacheSerializer.SerializeObject(_value));
#endif
                json = json.Replace(":", "-");
                return IncludePrefix ? GetPrefix() + json : json;
            }

            private string GetPrefix() => typeof(T).Name + "-";

        }

        protected abstract string? GetKey();

        string? IKeyGenerator.GetKey(string? expression, ISpringCachingRequirement requirement) => GetKey();

    }
}
