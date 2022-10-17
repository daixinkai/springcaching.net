using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public abstract class EmitValueDescriptor
#else
    internal abstract class EmitDescriptor
#endif

    {
        public abstract Type EmitValueType { get; }


        public abstract void EmitValue(ILGenerator iLGenerator, bool box);

        protected void EmitBox(ILGenerator iLGenerator, bool box)
        {
            //box
            if (box && EmitValueType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, EmitValueType);
            }
        }

    }

}
