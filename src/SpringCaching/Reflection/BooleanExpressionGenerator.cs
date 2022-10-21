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

            var tokenDescriptors = LogicalBooleanExpressionTokenDescriptor.FromTokens(tokens, descriptors);

            //var tokenDescriptors = BooleanExpressionTokenDescriptor.FromTokens(tokens, descriptors);

            List<EmitLocalBuilderDescriptor> tokenLocalBuilders = new List<EmitLocalBuilderDescriptor>();
            LocalBuilder? localBuilder = null;
            foreach (var tokenDescriptor in tokenDescriptors)
            {
                var tokenLocalBuilder = tokenDescriptor.EmitValue(iLGenerator, descriptors, ref localBuilder);
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
                return EmitExpressionResult.Success(localBuilder);
            }
            return EmitExpressionResult.Success(null);
        }

    }
}
