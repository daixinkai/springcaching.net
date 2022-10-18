using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpringCaching.Infrastructure;
using SpringCaching.Parsing;
using SpringCaching.Reflection;
using SpringCaching.Requirement;
using SpringCaching.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.UnitTest.NET45
{
    [TestClass]
    public class UnitTest_Parsing
    {
        [TestMethod]
        public void TestTT()
        {
            //string input = "{param.Id}";
            string input = "#param.Id.ToString() +  param.Id.ToString() +'Name' + \"asdasd\" ";
            InfixTokenizer infixTokenizer = new InfixTokenizer();
            var tokens = infixTokenizer.Tokenize(input);
            //process value
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token.Value == null)
                {
                    continue;
                }
                if (token.TokenType == ExpressionTokenType.Value && token.Value.StartsWith("#"))
                {
                    //#param.Id
                    // field                    
                    tokens[i] = new ExpressionToken()
                    {
                        TokenType = ExpressionTokenType.Field,
                        OperatorType = token.OperatorType
                    };
                    var rawToken = new KnownRawToken(token.RawToken?.TokenType ?? RawTokenType.None, token.RawToken?.Position ?? -1, token.Value.TrimStart('#'));
                    tokens[i].Append(rawToken);
                    tokens[i].Freeze();
                }
                else if (token.TokenType == ExpressionTokenType.Function && token.Value.StartsWith("#"))
                {
                    //#param.Id.ToString()
                    // function                    
                    tokens[i] = new ExpressionToken()
                    {
                        TokenType = ExpressionTokenType.Function,
                        OperatorType = token.OperatorType
                    };
                    var rawToken = new KnownRawToken(token.RawToken?.TokenType ?? RawTokenType.None, token.RawToken?.Position ?? -1, token.Value.TrimStart('#'));
                    tokens[i].Append(rawToken);
                    tokens[i].Freeze();
                }
                else if (token.TokenType == ExpressionTokenType.SingleQuoted && token.Value.Length > 1)
                {
                    //'Name'
                    tokens[i] = new ExpressionToken()
                    {
                        TokenType = ExpressionTokenType.DoubleQuoted,
                        OperatorType = token.OperatorType
                    };
                    var rawToken = new KnownRawToken(token.RawToken?.TokenType ?? RawTokenType.None, token.RawToken?.Position ?? -1, token.Value);
                    tokens[i].Append(rawToken);
                    tokens[i].Freeze();
                }
            }
            Assert.IsNotNull(tokens);
        }

        public void TestExpression1(TestServiceParam param)
        {
            string value = param.Id.ToString();
            value = value + param.Id;
            value = value + "Name";
            value = value + "asdasd";
        }


        public void TestExpression2(TestServiceParam param)
        {
            new CacheableRequirement("getNames")
            {
                KeyGenerator = new SimpleKeyGenerator.StringKeyGenerator(param.Id.ToString() + param.Id + "Name" + "asdasd", "null"),
                ExpirationPolicy = ExpirationPolicy.Absolute,
                ExpirationUnit = ExpirationUnit.Minute,
                ExpirationValue = 1
            };
        }

        private ICacheableRequirement GetCacheableRequirement_0(TestServiceParam param)
        {
            return new CacheableRequirement("getNames")
            {
                KeyGenerator = GetCacheableKeyGenerator_0(param),
                ExpirationPolicy = ExpirationPolicy.Absolute,
                ExpirationUnit = ExpirationUnit.Minute,
                ExpirationValue = 1
            };
        }

        private IKeyGenerator GetCacheableKeyGenerator_0(TestServiceParam param)
        {
            string value = param.Id.ToString() + param.Id + "Name" + "asdasd";
            return new SimpleKeyGenerator.StringKeyGenerator(value, "null");
        }

        private TestServiceParam _param;

        private string ToString<T>(T value) => value?.ToString();

        private IKeyGenerator GetCacheableKeyGenerator_00()
        {
            ////string num = ToString(this._param.Count);
            ////string name = this._param?.Param?.Name;
            ////string name = this._param?.Name;
            ////string name = ToString(this._param?.Param?.Name);
            ////string value = num + name;
            ////string value = (num ?? "null") + (name ?? "null") + (num ?? "null");
            //string name = this._param?.Name;
            //string value = null;
            //return new SimpleKeyGenerator.StringKeyGenerator(value, "null");
            //TestServiceParam testServiceParam = (this._param != null) ? this._param.Param : null;
            //int? value = (this._param.Param != null) ? this._param.Param.Id : null;
            //int? value = (this._param != null) ? this._param.Id : default;
            //var value = (this._param != null) ? this._param.Name : default;
            //string text = value;
            //string value2 = text ?? "null";
            //string value2 = _param?.Param?.Name + "-" + _param.Name;
            //string value2 = ToString(_param?.Param?.Id);
            //int? num = _param?.Id;
            //string value2 = ToString(num);
            TestServiceParam testServiceParam = (this._param != null) ? this._param.Param : null;
            TestServiceParam testServiceParam2 = testServiceParam;
            int? value = (testServiceParam2 != null) ? (int?)testServiceParam2.Count : null;
            string text = ToString(value);
            string value2 = text;
            return new SimpleKeyGenerator.StringKeyGenerator(value2, "null");
        }

        private IPredicateGenerator GetCacheableConditionGenerator_00()
        {
            bool value = _param.Count >= 0 && _param.Count != 0;
            return new PredicateGenerator(value);
        }

    }
}
