using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal class EmitExpressionResult
    {
        public bool Succeed { get; private set; }

        public LocalBuilder? LocalBuilder { get; private set; }

        public static EmitExpressionResult Success(LocalBuilder? localBuilder)
        {
            return new EmitExpressionResult
            {
                LocalBuilder = localBuilder,
                Succeed = true
            };
        }

        public static EmitExpressionResult Fail()
        {
            return new EmitExpressionResult();
        }

    }
}
