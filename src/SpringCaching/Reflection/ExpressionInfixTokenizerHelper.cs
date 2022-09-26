//using SpringCaching.Parsing;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SpringCaching.Reflection
//{

//    internal static class ExpressionInfixTokenizerHelper
//    {
//        public static void Test(string expression, IList<FieldBuilderDescriptor> fieldBuilders)
//        {
//            InfixTokenizer infixTokenizer = new InfixTokenizer();
//            var tokens = infixTokenizer.Tokenize(expression);
//            //process value
//            for (int i = 0; i < tokens.Length; i++)
//            {
//                var token = tokens[i];
//                if (token.TokenType == ExpressionTokenType.Value && token.Value != null && token.Value.StartsWith("#"))
//                {
//                    // field                    
//                    tokens[i] = new ExpressionToken()
//                    {
//                        TokenType = ExpressionTokenType.Field,
//                        OperatorType = token.OperatorType
//                    };
//                    var rawToken = new KnownRawToken(token.RawToken?.TokenType ?? RawTokenType.None, token.RawToken?.Position ?? -1, token.Value.TrimStart('#'));
//                    tokens[i].Append(rawToken);
//                    tokens[i].Freeze();
//                }
//            }
//        }
//    }
//}
