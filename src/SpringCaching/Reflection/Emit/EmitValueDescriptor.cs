using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal abstract class EmitValueDescriptor

    {
        public abstract Type EmitValueType { get; }


        //public abstract void EmitValue(ILGenerator iLGenerator, bool box);

        public abstract void EmitValue(ILGenerator iLGenerator);

        /// <summary>
        /// when type = int? :
        /// <para>excludeNullable = true : skip box</para>
        /// <para>excludeNullable = false : box</para>
        /// </summary>
        /// <param name="iLGenerator"></param>
        /// <param name="excludeNullable"></param>
        public void EmitBox(ILGenerator iLGenerator, bool excludeNullable)
        {
            //box
            if (!excludeNullable && EmitValueType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box, EmitValueType);
            }
            else if (EmitValueType.IsValueTypeEx())
            {
                iLGenerator.Emit(OpCodes.Box, EmitValueType);
            }
        }

    }

}
