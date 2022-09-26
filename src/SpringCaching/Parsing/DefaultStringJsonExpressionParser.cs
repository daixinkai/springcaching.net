#if NET45 || NETSTANDARD2_0
using JsonDocument = Newtonsoft.Json.Linq.JObject;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpringCaching.Parsing
{
    internal class DefaultStringJsonExpressionParser : DefaultJsonExpressionParser, IStringExpressionParser
    {
        public IParsedStringExpression Parse(string expression, IDictionary<string, object>? arguments)
        {
            if (arguments == null)
            {
                return new ParsedStringExpression(expression, false, null);
            }
            if (string.IsNullOrWhiteSpace(expression))
            {
                return new ParsedStringExpression(expression, false, null);
            }
            Dictionary<string, JsonDocument> jsonArguments = new Dictionary<string, JsonDocument>();
            //parse to json object            
            var value = Regex.Replace(expression, "#(.+)", match =>
            {
                string[] values = match.Value.Replace("#", "").Split('.');
                if (values.Length == 0)
                {
                    return match.Value;
                }
                string argumentName = values[0];
                if (!arguments.TryGetValue(argumentName, out var argument) || argument == null)
                {
                    return match.Value;
                }
                if (values.Length == 1)
                {
                    //if (argument is IList list && list != null)
                    //{
                    //    return string.Join(",", list.Cast<object>().ToArray());
                    //}
                    //return argument?.ToString();
                    return SerializeObject(argument);
                }
                List<string> paramList = values.ToList();
                paramList.RemoveAt(0);
                if (!jsonArguments.TryGetValue(argumentName, out var jObject))
                {
                    jObject = SerializeToDocument(argument);
                    jsonArguments.Add(argumentName, jObject);
                }

                var jsonElement = GetJsonElement(jObject, paramList);
                if (jsonElement == null)
                {
                    return "";
                }
#if NET45 || NETSTANDARD2_0
                return GetStringValue(jsonElement!) ?? "";
#else
                return GetStringValue(jsonElement!.Value) ?? "";
#endif

                //return GetValue(jObject, paramList) ?? "<null>";
            });
            if (string.IsNullOrWhiteSpace(value))
            {
                return new ParsedStringExpression(expression, false, null);
            }
            return new ParsedStringExpression(expression, true, value);
        }



    }
}
