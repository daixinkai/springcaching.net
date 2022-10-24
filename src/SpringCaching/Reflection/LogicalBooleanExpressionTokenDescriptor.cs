using SpringCaching.Internal;
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

        //public LogicalOperatorType? LogicalType { get; set; }

        //public LogicalOperatorType? ConnectLogicalType { get; set; }

        public bool IsLogicalOr { get; set; }

        public string Debug
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();



                return stringBuilder.ToString();
            }
        }

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
                    //current.NextLogicalType = tokenDescriptor.IsLogicalOr ? LogicalOperatorType.LogicalOr : LogicalOperatorType.LogicalAnd;

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


        public EmitLocalBuilderDescriptor? EmitValue(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors, ref LocalBuilder? localBuilder)
        {
            if (ExpressionTokenDescriptors.Count == 1 && localBuilder == null)
            {
                var emitResult = ExpressionTokenDescriptors[0].EmitValue(iLGenerator, descriptors);
                if (emitResult != null)
                {
                    localBuilder = emitResult!.LocalBuilder;
                }
                return emitResult;
            }

            if (localBuilder == null)
            {
                localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            }

            Label? logicalOrElseLabel = null;
            if (IsLogicalOr)
            {
                logicalOrElseLabel = iLGenerator.DefineLabel();
                iLGenerator.Emit(OpCodes.Ldloc, localBuilder);
                iLGenerator.Emit(OpCodes.Ldc_I4_0);
                iLGenerator.Emit(OpCodes.Bgt_S, logicalOrElseLabel.Value);
            }
            var elseLabel = iLGenerator.DefineLabel();
            var lastLabel = iLGenerator.DefineLabel();

            for (int i = 0; i < ExpressionTokenDescriptors.Count; i++)
            {
                var expressionTokenDescriptor = ExpressionTokenDescriptors[i];
                bool isLast = i == ExpressionTokenDescriptors.Count - 1;
                var emitLocalBuilderDescriptor = expressionTokenDescriptor.EmitValue(iLGenerator, descriptors);
                emitLocalBuilderDescriptor!.EmitValue(iLGenerator);
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


            if (IsLogicalOr)
            {
                iLGenerator.MarkLabel(logicalOrElseLabel!.Value);
                iLGenerator.Emit(OpCodes.Ldc_I4_1);
            }

            iLGenerator.Emit(OpCodes.Stloc, localBuilder);

            return new EmitLocalBuilderDescriptor(localBuilder!);
        }

    }
}
