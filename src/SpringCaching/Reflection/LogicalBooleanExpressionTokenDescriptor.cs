using SpringCaching.Parsing;
using SpringCaching.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal class LogicalBooleanExpressionTokenDescriptor
    {

        public enum LogicalOperatorType
        {
            /// <summary>
            /// &amp;&amp; between values.
            /// </summary>
            LogicalAnd,
            /// <summary>
            /// || between values.
            /// </summary>
            LogicalOr,
            /// <summary>
            /// ! before a value.
            /// </summary>
            LogicalNegation,
        }

        public List<BooleanExpressionTokenDescriptor> ExpressionTokenDescriptors { get; } = new List<BooleanExpressionTokenDescriptor>();

        public LogicalOperatorType? LogicalType { get; set; }

        public LogicalOperatorType? ConnectLogicalType { get; set; }

        public LogicalBooleanExpressionTokenDescriptor? Next { get; set; }

        public string Debug
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();



                return stringBuilder.ToString();
            }
        }

        public static List<LogicalBooleanExpressionTokenDescriptor> FromTokens(IList<ExpressionToken> tokens, IList<EmitFieldBuilderDescriptor>? fieldBuilderDescriptors)
        {

            var tokenDescriptors = BooleanExpressionTokenDescriptor.FromTokens(tokens, fieldBuilderDescriptors);

            var logicalTokenDescriptors = new List<LogicalBooleanExpressionTokenDescriptor>();

            LogicalBooleanExpressionTokenDescriptor current = new LogicalBooleanExpressionTokenDescriptor();

            foreach (var tokenDescriptor in tokenDescriptors)
            {
                string debug = tokenDescriptor.Debug;
                current.ExpressionTokenDescriptors.Add(tokenDescriptor);
                if (tokenDescriptor.OpenParenthesisToken != null)
                {
                    current.ExpressionTokenDescriptors.Add(tokenDescriptor);
                }
                if (tokenDescriptor.ConnectOperatorToken != null)
                {
                    if (current.ExpressionTokenDescriptors.Count == 1 && !current.ConnectLogicalType.HasValue)
                    {
                        current.ConnectLogicalType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), tokenDescriptor.ConnectOperatorToken.OperatorType.ToString());
                    }
                    else if (current.ExpressionTokenDescriptors.Count > 1 && !current.LogicalType.HasValue)
                    {
                        current.LogicalType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), tokenDescriptor.ConnectOperatorToken.OperatorType.ToString());
                        logicalTokenDescriptors.Add(current);
                        //close
                        current = new LogicalBooleanExpressionTokenDescriptor();
                    }
                }
                if (tokenDescriptor.CloseParenthesisToken != null)
                {
                    logicalTokenDescriptors.Add(current);
                    //close
                    current = new LogicalBooleanExpressionTokenDescriptor();
                }
            }

            if (!logicalTokenDescriptors.Contains(current) && current.ExpressionTokenDescriptors.Count > 0)
            {
                logicalTokenDescriptors.Add(current);
            }

            for (int i = 0; i < logicalTokenDescriptors.Count; i++)
            {
                if (i < logicalTokenDescriptors.Count - 1)
                {
                    logicalTokenDescriptors[i].Next = logicalTokenDescriptors[i + 1];
                }
            }

            return logicalTokenDescriptors;

        }


        public EmitLocalBuilderDescriptor? EmitValue(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (!LogicalType.HasValue)
            {
                return ExpressionTokenDescriptors[0].EmitValue(iLGenerator, descriptors);
            }



            return null;
        }

    }
}
