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

        public LogicalBooleanExpressionTokenDescriptor? Next { get; set; }

        public LogicalOperatorType? NextLogicalType { get; set; }

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
                    //begin
                }
                if (tokenDescriptor.IsLogicalOr)
                {
                    logicalTokenDescriptors.Add(current);
                    //close
                    current = new LogicalBooleanExpressionTokenDescriptor();
                }
                //if (tokenDescriptor.ConnectOperatorToken != null)
                //{
                //    if (current.ExpressionTokenDescriptors.Count == 1 && !current.ConnectLogicalType.HasValue)
                //    {
                //        current.ConnectLogicalType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), tokenDescriptor.ConnectOperatorToken.OperatorType.ToString());
                //    }
                //    else if (current.ExpressionTokenDescriptors.Count > 1 && !current.LogicalType.HasValue)
                //    {
                //        current.LogicalType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), tokenDescriptor.ConnectOperatorToken.OperatorType.ToString());
                //        logicalTokenDescriptors.Add(current);
                //        //close
                //        //current = new LogicalBooleanExpressionTokenDescriptor();
                //    }
                //}
                if (tokenDescriptor.CloseParenthesisToken != null)
                {
                    current.NextLogicalType = tokenDescriptor.IsLogicalOr ? LogicalOperatorType.LogicalOr : LogicalOperatorType.LogicalAnd;
                    logicalTokenDescriptors.Add(current);
                    //close
                    current = new LogicalBooleanExpressionTokenDescriptor();
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
            for (int i = 0; i < logicalTokenDescriptors.Count; i++)
            {
                var current = logicalTokenDescriptors[i];
                if (i < logicalTokenDescriptors.Count - 1)
                {
                    var next = logicalTokenDescriptors[i + 1];
                    if (current.NextLogicalType.HasValue && current.NextLogicalType!.Value == LogicalOperatorType.LogicalAnd)
                    {
                        //Merge
                        current.ExpressionTokenDescriptors.AddRange(next.ExpressionTokenDescriptors);
                        current.Next = next.Next;
                        current.NextLogicalType = next.NextLogicalType;
                        i--;
                        logicalTokenDescriptors.Remove(next);
                    }
                    else
                    {
                        current.Next = next;
                    }
                }
            }
        }




        public EmitLocalBuilderDescriptor? EmitValue(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors, ref LocalBuilder? localBuilder)
        {
            if (ExpressionTokenDescriptors.Count == 1)
            {
                var emitResult = ExpressionTokenDescriptors[0].EmitValue(iLGenerator, descriptors);
                if (emitResult != null)
                {
                    localBuilder = emitResult!.LocalBuilder;
                }
                //EmitNext(iLGenerator, descriptors, ref localBuilder);
                return emitResult;
            }
            //if (!LogicalType.HasValue)
            //{
            //    var emitResult = ExpressionTokenDescriptors[0].EmitValue(iLGenerator, descriptors);
            //    if (emitResult != null)
            //    {
            //        localBuilder = emitResult!.LocalBuilder;
            //    }
            //    return emitResult;
            //}


            if (localBuilder == null)
            {
                localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            }

            Label elseLabel = iLGenerator.DefineLabel();

            Label lastLabel = iLGenerator.DefineLabel();

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

            iLGenerator.Emit(OpCodes.Stloc, localBuilder);

            return new EmitLocalBuilderDescriptor(localBuilder);
        }

        private void EmitNext(ILGenerator iLGenerator, IList<EmitFieldBuilderDescriptor> descriptors, ref LocalBuilder? localBuilder)
        {
            if (Next == null || !NextLogicalType.HasValue)
            {
                return;
            }

        }

    }
}
