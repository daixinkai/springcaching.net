using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal abstract class EmitValueDescriptor

    {
        public abstract Type EmitValueType { get; }


        //public abstract void EmitValue(ILGenerator iLGenerator, bool box);

        public abstract void EmitValue(ILGenerator iLGenerator);

        public void EmitBox(ILGenerator iLGenerator)
        {
            //box
            if (EmitValueType.IsValueTypeEx())
            {
                iLGenerator.Emit(OpCodes.Box, EmitValueType);
            }
        }

    }

}
