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
    internal static class BooleanExpressionGenerator
    {

        public static EmitExpressionResult EmitExpression(ILGenerator iLGenerator, string expression, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            var tokens = ExpressionTokenHelper.ParseExpressionTokens(expression);

            var tokenDescriptors = BooleanExpressionTokenDescriptor.FromTokens(tokens, descriptors);

            List<EmitLocalBuilderDescriptor> tokenLocalBuilders = new List<EmitLocalBuilderDescriptor>();
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
                return EmitExpressionResult.Fail();
            }
            if (tokenLocalBuilders.Count == 1)
            {
                return EmitExpressionResult.Success(tokenLocalBuilders[0].LocalBuilder);
            }
            //localBuilder = iLGenerator.DeclareLocal(typeof(bool));
            EmitBooleanPredicate(iLGenerator, tokenLocalBuilders);
            //iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            return EmitExpressionResult.Success(null);
        }


        private static void EmitBooleanPredicate(ILGenerator iLGenerator, IList<EmitLocalBuilderDescriptor> descriptors)
        {
            if (descriptors.Count == 1)
            {
                descriptors[0].EmitValue(iLGenerator);
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
