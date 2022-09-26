using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpringCaching.Parsing;
using SpringCaching.Reflection;
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
            string input = "#param.Id + {param.Id} + 'Name' + \"asdasd\" ";
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
                else if (token.TokenType == ExpressionTokenType.SingleQuoted && token.Value.Length > 1)
                {
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
    }
}
