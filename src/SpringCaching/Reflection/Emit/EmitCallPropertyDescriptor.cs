using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection.Emit
{
    internal class EmitCallPropertyDescriptor
    {
        public EmitCallPropertyDescriptor(EmitValueDescriptor descriptor, List<EmitPropertyDescriptor> propertyDescriptors)
        {
            Descriptor = descriptor;
            PropertyDescriptors = propertyDescriptors;

            EmitValueDescriptor = propertyDescriptors.Count > 0 ?
               propertyDescriptors[propertyDescriptors.Count - 1] :
               descriptor;

        }
        public EmitValueDescriptor Descriptor { get; }
        public List<EmitPropertyDescriptor> PropertyDescriptors { get; }

        public EmitValueDescriptor EmitValueDescriptor { get; }

        public Type EmitValueType => EmitValueDescriptor.EmitValueType;


        public EmitValueDescriptor EmitValue(ILGenerator iLGenerator)
        {
            Descriptor.EmitValue(iLGenerator);
            EmitValueDescriptor parentDescriptor = Descriptor;
            foreach (var descriptor in PropertyDescriptors)
            {
                descriptor.ParentDescriptor = parentDescriptor;
                descriptor.EmitValue(iLGenerator);
                parentDescriptor = descriptor;
            }
            return parentDescriptor;
        }


    }
}
