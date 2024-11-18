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

        public static EmitExpressionResult EmitExpression(ILGenerator iLGenerator, string expression, IList<EmitParameterValueDescriptor> descriptors)
        {
            var parsedTokens = ExpressionTokenHelper.ParseExpressionTokens(expression);
            var tokenDescriptors = LogicalBooleanExpressionTokenDescriptor.FromTokens(parsedTokens, descriptors);
            return LogicalBooleanExpressionTokenDescriptor.EmitValue(iLGenerator, tokenDescriptors, descriptors);
        }

    }
}
