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
            public StringKeyGenerator(string? value, string? nullValue)
            {
                _value = value;
                _nullValue = nullValue;
            }

            private readonly string? _value;
            private readonly string? _nullValue;

            protected override string? GetKey()
            {
                if (_value == null)
                {
                    return _nullValue;
                }
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
            public NullableToStringKeyGenerator(T? value, string? nullValue)
            {
                _value = value;
                _nullValue = nullValue;
            }

            private readonly T? _value;
            private readonly string? _nullValue;
            protected override string? GetKey()
            {
                if (!_value.HasValue)
                {
                    return _nullValue;
                }
                return _value.Value.ToString();
            }
        }

        public class JsonKeyGenerator<T> : SimpleKeyGenerator
        {
            public JsonKeyGenerator(T? value, string? nullValue)
            {
                _value = value;
                _nullValue = nullValue;
            }

            private readonly T? _value;
            private readonly string? _nullValue;

            public bool IncludePrefix { get; set; }

            protected override string? GetKey()
            {
                if (_value == null)
                {
                    return IncludePrefix ? GetPrefix() + _nullValue : _nullValue;
                }
#if NET45 || NETSTANDARD2_0
                string json = Encoding.UTF8.GetString(NewtonsoftJsonCacheSerializer.JsonCacheSerializer.SerializeObject(_value));
#else
                string json = Encoding.UTF8.GetString(SystemTextJsonCacheSerializer.JsonCacheSerializer.SerializeObject(_value));
#endif
                return IncludePrefix ? GetPrefix() + json : json;
            }

            private string GetPrefix() => typeof(T).Name + "-";

        }

        protected abstract string? GetKey();

        string? IKeyGenerator.GetKey(string? expression, IStringExpressionParser parser, ISpringCachingRequirement requirement) => GetKey();

    }
}
