using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public class EmitPropertyDescriptor : EmitValueDescriptor
#else
    internal class PropertyEmitDescriptor : EmitDescriptor
#endif
    {
        public EmitPropertyDescriptor(PropertyInfo property, bool ignoreNull)
        {
            Property = property;
            IgnoreNull = ignoreNull;
        }
        public bool IgnoreNull { get; }
        public PropertyInfo Property { get; }

        public override Type EmitValueType => Property.PropertyType;

        public LocalBuilder? LocalBuilder { get; private set; }

        public EmitValueDescriptor? ParentDescriptor { get; set; }

        public override void EmitValue(ILGenerator iLGenerator, bool box)
        {
            if (IgnoreNull)
            {
                iLGenerator.Emit(OpCodes.Callvirt, Property.GetMethod!);
                EmitBox(iLGenerator, box);
                return;
            }

            LocalBuilder = iLGenerator.DeclareLocal(Property.PropertyType);
            Label trueLabel = iLGenerator.DefineLabel();
            Label falseLabel = iLGenerator.DefineLabel();
            if (ParentDescriptor is EmitPropertyDescriptor)
            {           
                iLGenerator.Emit(OpCodes.Dup);
            }
            iLGenerator.Emit(OpCodes.Brtrue_S, trueLabel);
            if (ParentDescriptor is EmitPropertyDescriptor)
            {
                iLGenerator.Emit(OpCodes.Pop);
            }
            iLGenerator.Emit(OpCodes.Ldnull);
            iLGenerator.Emit(OpCodes.Br_S, falseLabel);

            iLGenerator.MarkLabel(trueLabel);
            ParentDescriptor!.EmitValue(iLGenerator, box);
            iLGenerator.Emit(OpCodes.Callvirt, Property.GetMethod!);
            EmitBox(iLGenerator, box);
            iLGenerator.MarkLabel(falseLabel);

            iLGenerator.Emit(OpCodes.Stloc, LocalBuilder);
            iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);

        }


        public static void EmitValue(ILGenerator iLGenerator, FieldBuilderDescriptor fieldDescriptor, List<EmitPropertyDescriptor> descriptors)
        {
            fieldDescriptor.EmitValue(iLGenerator, false);
            EmitValueDescriptor parentDescriptor = fieldDescriptor;
            foreach (var descriptor in descriptors)
            {
                descriptor.ParentDescriptor = parentDescriptor;
                descriptor.EmitValue(iLGenerator, false);
                parentDescriptor = descriptor;
            }
        }

    }
}
