using SpringCaching.Internal;
using SpringCaching.Parsing;
using SpringCaching.Reflection.Emit;
using System;
using System.Collections;
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

        public bool IsLogicalOr { get; set; }

        public LogicalBooleanExpressionTokenDescriptor? Next { get; set; }

        public static List<LogicalBooleanExpressionTokenDescriptor> FromTokens(IList<ParsedExpressionToken> parsedTokens, IList<EmitFieldBuilderDescriptor>? fieldBuilderDescriptors)
        {

            var tokenDescriptors = BooleanExpressionTokenDescriptor.FromTokens(parsedTokens, fieldBuilderDescriptors);

            var logicalTokenDescriptors = new List<LogicalBooleanExpressionTokenDescriptor>();

            LogicalBooleanExpressionTokenDescriptor current = new LogicalBooleanExpressionTokenDescriptor();

            foreach (var tokenDescriptor in tokenDescriptors)
            {
                string debug = tokenDescriptor.Debug;
                if (tokenDescriptor.IsLogicalOr)
                {
                    var temp = new LogicalBooleanExpressionTokenDescriptor();
                    if (current.ExpressionTokenDescriptors.Count > 0)
                    {
                        temp.IsLogicalOr = true;
                        logicalTokenDescriptors.Add(current);
                    }
                    current = temp;
                }
                current.ExpressionTokenDescriptors.Add(tokenDescriptor);
                if (tokenDescriptor.OpenParenthesisToken != null)
                {
                    //begin
                }
                if (tokenDescriptor.CloseParenthesisToken != null)
                {
                    var temp = new LogicalBooleanExpressionTokenDescriptor();
                    if (tokenDescriptor.CloseParenthesisToken.NextToken != null)
                    {
                        temp.IsLogicalOr = true;
                    }
                    logicalTokenDescriptors.Add(current);
                    //close
                    current = temp;
                }
            }

            if (!logicalTokenDescriptors.Contains(current) && current.ExpressionTokenDescriptors.Count > 0)
            {
                logicalTokenDescriptors.Add(current);
            }
            Merge(logicalTokenDescriptors);
            return logicalTokenDescriptors;
        }

        private static void Merge(List<LogicalBooleanExpressionTokenDescriptor> logicalTokenDescriptors)
        {
            ArrayEx.ForEach(logicalTokenDescriptors, (current, next) =>
            {
                current.Next = next;
                if (next == null)
                {
                    return false;
                }
                if (!next.IsLogicalOr)
                {
                    current.ExpressionTokenDescriptors.AddRange(next.ExpressionTokenDescriptors);
                    return true;
                }
                return false;
                //if (current.NextLogicalType.HasValue && current.NextLogicalType!.Value == LogicalOperatorType.LogicalAnd)
                //{
                //    //Merge
                //    current.ExpressionTokenDescriptors.AddRange(next.ExpressionTokenDescriptors);
                //    current.Next = next.Next;
                //    current.NextLogicalType = next.NextLogicalType;
                //    return true;
                //}
                //else
                //{
                //    current.Next = next;
                //    return false;
                //}
            });
        }

        private void EmitValue(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (ExpressionTokenDescriptors.Count == 1)
            {
                ExpressionTokenDescriptors[0].EmitValue(iLGenerator, descriptors);
                return;
            }


            var elseLabel = iLGenerator.DefineLabel();
            var lastLabel = iLGenerator.DefineLabel();

            for (int i = 0; i < ExpressionTokenDescriptors.Count; i++)
            {
                var expressionTokenDescriptor = ExpressionTokenDescriptors[i];
                bool isLast = i == ExpressionTokenDescriptors.Count - 1;
                expressionTokenDescriptor.EmitValue(iLGenerator, descriptors);
                if (!isLast)
                {
                    iLGenerator.Emit(OpCodes.Brfalse_S, elseLabel);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Br_S, lastLabel);
                    iLGenerator.MarkLabel(elseLabel);
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    iLGenerator.MarkLabel(lastLabel);
                }
            }


        }

        public static EmitExpressionResult EmitValue(ILGenerator iLGenerator, IList<LogicalBooleanExpressionTokenDescriptor> logicalDescriptors, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (logicalDescriptors.Count == 0)
            {
                return EmitExpressionResult.Fail();
            }
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(bool));

            Label? logicalOrLabel = null;

            foreach (var logicalDescriptor in logicalDescriptors)
            {
                logicalDescriptor.EmitValue(iLGenerator, descriptors);
                if (logicalDescriptor.Next != null)
                {
                    if (!logicalOrLabel.HasValue)
                    {
                        logicalOrLabel = iLGenerator.DefineLabel();
                    }
                    iLGenerator.Emit(OpCodes.Brtrue_S, logicalOrLabel.Value);
                    continue;
                }
                if (logicalOrLabel.HasValue)
                {
                    var lastLabel = iLGenerator.DefineLabel();
                    iLGenerator.Emit(OpCodes.Br_S, lastLabel);
                    iLGenerator.MarkLabel(logicalOrLabel.Value);
                    iLGenerator.Emit(OpCodes.Ldc_I4_1);
                    iLGenerator.MarkLabel(lastLabel);
                }
            }

            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return EmitExpressionResult.Success(localBuilder);
        }

    }
}
