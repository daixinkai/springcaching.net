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
                KeyGenerator = new SimpleKeyGenerator.StringKeyGenerator(param.Id.ToString() + param.Id + "Name" + "asdasd", "null"),
                ExpirationPolicy = ExpirationPolicy.Absolute,
                ExpirationUnit = ExpirationUnit.Minute,
                ExpirationValue = 1
            };
        }

    }
}
