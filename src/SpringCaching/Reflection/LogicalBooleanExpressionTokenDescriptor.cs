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

        public bool IsLogicalNegation { get; set; }

        public LogicalBooleanExpressionTokenDescriptor? Next { get; set; }

        public string Debug
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (IsLogicalNegation)
                {
                    stringBuilder.Append("!(");
                }
                foreach (var item in ExpressionTokenDescriptors)
                {
                    stringBuilder.Append(item.Debug + " && ");
                }
                if (ExpressionTokenDescriptors.Count > 0)
                {
                    stringBuilder = stringBuilder.Remove(stringBuilder.Length - 4, 4);
                }
                if (IsLogicalNegation)
                {
                    stringBuilder.Append(")");
                }
                return stringBuilder.ToString();
            }
        }

        public static List<LogicalBooleanExpressionTokenDescriptor> FromTokens(IList<ParsedExpressionToken> parsedTokens, IList<EmitParameterValueDescriptor>? parameterBuilderDescriptors)
        {

            var tokenDescriptors = BooleanExpressionTokenDescriptor.FromTokens(parsedTokens, parameterBuilderDescriptors);

            var logicalTokenDescriptors = new List<LogicalBooleanExpressionTokenDescriptor>();

            LogicalBooleanExpressionTokenDescriptor current = new LogicalBooleanExpressionTokenDescriptor();

            foreach (var tokenDescriptor in tokenDescriptors)
            {
#if DEBUG
                string debug = tokenDescriptor.Debug;
#endif
                if (tokenDescriptor.IsLogicalOr)
                {
                    var temp = new LogicalBooleanExpressionTokenDescriptor();
                    temp.IsLogicalOr = true;
                    if (current.ExpressionTokenDescriptors.Count > 0)
                    {
                        logicalTokenDescriptors.Add(current);
                    }
                    current = temp;
                }
                current.ExpressionTokenDescriptors.Add(tokenDescriptor);
                if (tokenDescriptor.OpenParenthesisToken != null)
                {
                    //begin
                    current.IsLogicalNegation = tokenDescriptor.IsLogicalNegation;
                }
                if (tokenDescriptor.CloseParenthesisToken != null)
                {
                    var temp = new LogicalBooleanExpressionTokenDescriptor();
                    if (tokenDescriptor.CloseParenthesisToken.NextToken != null)
                    {
                        temp.IsLogicalOr = tokenDescriptor.CloseParenthesisToken.NextToken.OperatorType == OperatorType.LogicalOr;
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
                if (current.ExpressionTokenDescriptors.Count > 0 && current.IsLogicalNegation)
                {
                    if (current.ExpressionTokenDescriptors.Count == 1)
                    {
                        current.IsLogicalNegation = false;
                    }
                    else
                    {
                        current.ExpressionTokenDescriptors[0].IsLogicalNegation = false;
                    }
                }
                current.Next = next;
                if (next == null)
                {
                    return false;
                }
                if (next.IsLogicalOr)
                {
                    return false;
                }
                if (current.IsLogicalNegation)
                {
                    return false;
                }
                current.ExpressionTokenDescriptors.AddRange(next.ExpressionTokenDescriptors);
                return true;
            });
        }

        private void EmitValue(ILGenerator iLGenerator, IList<EmitParameterValueDescriptor> descriptors)
        {

#if DEBUG
            string debug = Debug;
#endif

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
#if DEBUG
                string tokenDebug = expressionTokenDescriptor.Debug;
#endif
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
                    if (IsLogicalNegation)
                    {
                        ExpressionTokenHelper.EmitOperatorType(iLGenerator, OperatorType.LogicalNegation);
                    }
                }
            }

        }

        public static EmitExpressionResult EmitValue(ILGenerator iLGenerator, IList<LogicalBooleanExpressionTokenDescriptor> logicalDescriptors, IList<EmitParameterValueDescriptor> descriptors)
        {
            if (logicalDescriptors.Count == 0)
            {
                return EmitExpressionResult.Fail();
            }
            LocalBuilder localBuilder = iLGenerator.DeclareLocal(typeof(bool));

            Label? logicalOrLabel = null;

            Label? logicalAndLabel = null;

            foreach (var logicalDescriptor in logicalDescriptors)
            {
                logicalDescriptor.EmitValue(iLGenerator, descriptors);
                if (logicalDescriptor.Next != null)
                {
                    if (logicalDescriptor.Next.IsLogicalOr)
                    {
                        if (!logicalOrLabel.HasValue)
                        {
                            logicalOrLabel = iLGenerator.DefineLabel();
                        }
                        iLGenerator.Emit(OpCodes.Brtrue_S, logicalOrLabel.Value);
                    }
                    else
                    {
                        if (!logicalAndLabel.HasValue)
                        {
                            logicalAndLabel = iLGenerator.DefineLabel();
                        }
                        iLGenerator.Emit(OpCodes.Brfalse_S, logicalAndLabel.Value);
                    }
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
                if (logicalAndLabel.HasValue)
                {
                    var lastLabel = iLGenerator.DefineLabel();
                    iLGenerator.Emit(OpCodes.Br_S, lastLabel);
                    iLGenerator.MarkLabel(logicalAndLabel.Value);
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    iLGenerator.MarkLabel(lastLabel);
                }
            }

            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return EmitExpressionResult.Success(localBuilder);
        }

    }
}
