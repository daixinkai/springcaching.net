using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitExpressionResult
    {
        public bool Succeed { get; private set; }

        public LocalBuilder? LocalBuilder { get; private set; }

        public Type? Type { get; set; }

        public static EmitExpressionResult Success()
        {
            return new EmitExpressionResult
            {
                Succeed = true
            };
        }

        public static EmitExpressionResult Success(LocalBuilder localBuilder)
        {
            return new EmitExpressionResult
            {
                LocalBuilder = localBuilder,
                Succeed = true
            };
        }

        public static EmitExpressionResult Success(Type type)
        {
            return new EmitExpressionResult
            {
                Type = type,
                Succeed = true
            };
        }

        public static EmitExpressionResult Fail()
        {
            return new EmitExpressionResult();
        }

    }
}
