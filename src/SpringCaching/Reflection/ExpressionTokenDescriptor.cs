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
    internal class ExpressionTokenDescriptor
    {
        public ExpressionTokenDescriptor(ExpressionToken token, IList<EmitFieldBuilderDescriptor>? descriptors)
        {
            Token = token;
            if (descriptors != null)
            {
                TrySetTokenValueType(descriptors);
            }
        }

        public ExpressionToken Token { get; }

        public ExpressionTokenType TokenType => Token.TokenType;

        public Type? TokenValueType { get; private set; }

        public EmitCallPropertyDescriptor? EmitCallPropertyDescriptor { get; private set; }

        public void TrySetTokenValueType(IList<EmitFieldBuilderDescriptor> descriptors)
        {

            var simpleType = TokenType switch
            {
                ExpressionTokenType.SingleQuoted => typeof(string),
                ExpressionTokenType.DoubleQuoted => typeof(string),
                _ => null
            };

            if (simpleType != null)
            {
                TokenValueType = simpleType;
                return;
            }


            if (TokenType == ExpressionTokenType.Field)
            {
                EmitCallPropertyDescriptor = ExpressionTokenHelper.GetEmitCallPropertyDescriptor(Token, descriptors);
                if (EmitCallPropertyDescriptor == null)
                {
                    return;
                }
                TokenValueType = EmitCallPropertyDescriptor.EmitValueType;
            }
            else if (TokenType == ExpressionTokenType.Value)
            {

            }
        }

    }
}
