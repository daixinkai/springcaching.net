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
        public EmitCallPropertyDescriptor(EmitFieldBuilderDescriptor fieldDescriptor, List<EmitPropertyDescriptor> propertyDescriptors)
        {
            FieldDescriptor = fieldDescriptor;
            PropertyDescriptors = propertyDescriptors;

            EmitValueDescriptor = propertyDescriptors.Count > 0 ?
               propertyDescriptors[propertyDescriptors.Count - 1] :
               fieldDescriptor;

        }
        public EmitFieldBuilderDescriptor FieldDescriptor { get; }
        public List<EmitPropertyDescriptor> PropertyDescriptors { get; }

        public EmitValueDescriptor EmitValueDescriptor { get; }

        public Type EmitValueType => EmitValueDescriptor.EmitValueType;


        public EmitValueDescriptor EmitValue(ILGenerator iLGenerator)
        {
            FieldDescriptor.EmitValue(iLGenerator);
            EmitValueDescriptor parentDescriptor = FieldDescriptor;
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
