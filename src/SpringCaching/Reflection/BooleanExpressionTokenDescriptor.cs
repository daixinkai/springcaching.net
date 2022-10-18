using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal class BooleanExpressionTokenDescriptor
    {

        public BooleanExpressionTokenDescriptor(ExpressionToken left)
        {
            Left = left;
        }

        public enum ExpressionType
        {
            Value,
            Compare
        }

        public ExpressionToken Left { get; }

        public ExpressionType Type => Compare == null ? ExpressionType.Value : ExpressionType.Compare;

        public ExpressionToken? Compare { get; set; }
        public ExpressionToken? Right { get; set; }


        public bool IsCompleted => Compare == null && Right == null || Compare != null && Right != null;

        public static List<BooleanExpressionTokenDescriptor> FromTokens(IList<ExpressionToken> tokens)
        {
            List<BooleanExpressionTokenDescriptor> descriptors = new List<BooleanExpressionTokenDescriptor>();

            BooleanExpressionTokenDescriptor? currentDescriptor = null;

            foreach (var token in tokens)
            {
                if (token.TokenType == ExpressionTokenType.Field
                    || token.TokenType == ExpressionTokenType.Value
                    )
                {
                    if (currentDescriptor == null || currentDescriptor.Compare == null)
                    {
                        currentDescriptor = new BooleanExpressionTokenDescriptor(token);
                    }
                    else
                    {
                        currentDescriptor.Right = token;
                        descriptors.Add(currentDescriptor);
                        currentDescriptor = null;
                    }
                    continue;
                }
                else if (token.TokenType == ExpressionTokenType.Operator)
                {
                    if ((token.OperatorType == OperatorType.LogicalAnd || token.OperatorType == OperatorType.LogicalOr) && currentDescriptor != null)
                    {
                        descriptors.Add(currentDescriptor);
                        currentDescriptor = null;
                    }
                    if (currentDescriptor != null && currentDescriptor.Compare == null)
                    {
                        currentDescriptor.Compare = token;
                    }
                }
            }
            if (currentDescriptor != null && currentDescriptor.IsCompleted)
            {
                descriptors.Add(currentDescriptor);
            }
            return descriptors;
        }

    }
}
