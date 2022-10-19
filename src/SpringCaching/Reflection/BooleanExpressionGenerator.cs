﻿using SpringCaching.Internal;
using SpringCaching.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public static class BooleanExpressionGenerator
#else
    internal static class BooleanExpressionGenerator
#endif
    {


        public static bool EmitExpression(ILGenerator iLGenerator, string expression, IList<FieldBuilderDescriptor> descriptors, out LocalBuilder? localBuilder)
        {
            localBuilder = null;
            var tokens = ExpressionTokenHelper.ParseExpressionTokens(expression);

            var tokenDescriptors = BooleanExpressionTokenDescriptor.FromTokens(tokens);

            List<LocalBuilderDescriptor> tokenLocalBuilders = new List<LocalBuilderDescriptor>();
            foreach (var tokenDescriptor in tokenDescriptors)
            {
                var tokenLocalBuilder = tokenDescriptor.EmitValue(iLGenerator, descriptors);
                if (tokenLocalBuilder != null)
                {
                    tokenLocalBuilders.Add(tokenLocalBuilder);
                }
            }
            if (tokenLocalBuilders.Count == 0)
            {
                return false;
            }
            if (tokenLocalBuilders.Count == 1)
            {
                localBuilder = tokenLocalBuilders[0].LocalBuilder;
                return true;
            }
            //localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            EmitBooleanPredicate(iLGenerator, tokenLocalBuilders);
            //iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return true;
        }

  
        private static void EmitBooleanPredicate(ILGenerator iLGenerator, IList<LocalBuilderDescriptor> descriptors)
        {
            if (descriptors.Count == 1)
            {
                descriptors[0].EmitValue(iLGenerator, false);
                return;
            }
            ////iLGenerator.Emit(OpCodes.Ldloc, tokenLocalBuilders[0].LocalBuilder);
            //localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            ////EmitConcatString(iLGenerator, tokenLocalBuilders);
            //iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            iLGenerator.Emit(OpCodes.Ldloc, descriptors[0].LocalBuilder);
        }




    }
}
