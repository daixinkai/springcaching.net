﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal class EmitPropertyDescriptor : EmitValueDescriptor
    {
        public EmitPropertyDescriptor(PropertyInfo property, bool ignoreNull)
        {
            Property = property;
            IgnoreNull = ignoreNull;
        }
        public bool IgnoreNull { get; }
        public PropertyInfo Property { get; }

        public override Type EmitValueType
        {
            get
            {
                if (Property.PropertyType.IsValueTypeEx() && IsCheckNull(this))
                {
                    return typeof(Nullable<>).MakeGenericType(Property.PropertyType);
                }
                return Property.PropertyType;
            }
        }

        public LocalBuilder? LocalBuilder { get; private set; }

        public EmitValueDescriptor? ParentDescriptor { get; set; }

        public bool IsLast { get; set; }

        public override void EmitValue(ILGenerator iLGenerator)
        {
            if (IgnoreNull)
            {
                //if (ParentDescriptor != null && !ParentDescriptor.EmitValueType.IsNullableType())
                //{
                EmitValueInternal(iLGenerator);
                return;
                //}
            }
            if (LocalBuilder != null)
            {
                iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
                return;
            }
            LocalBuilder = iLGenerator.DeclareLocal(EmitValueType);
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
            if (LocalBuilder!.LocalType.IsNullableType())
            {
                iLGenerator.Emit(OpCodes.Ldloca, LocalBuilder);
                iLGenerator.Emit(OpCodes.Initobj, LocalBuilder!.LocalType);
                iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            iLGenerator.Emit(OpCodes.Br_S, falseLabel);
            iLGenerator.MarkLabel(trueLabel);
            if (ParentDescriptor is EmitPropertyDescriptor emitPropertyDescriptor && emitPropertyDescriptor.LocalBuilder != null)
            {
                //skip                
            }
            else
            {
                ParentDescriptor!.EmitValue(iLGenerator);
            }
            EmitValueInternal(iLGenerator);
            if (Property.PropertyType.IsValueTypeEx() && LocalBuilder!.LocalType.IsNullableType())
            {
                iLGenerator.Emit(OpCodes.Newobj, LocalBuilder!.LocalType.GetConstructors()[0]);
            }
            if (IsLast)
            {
                EmitBox(iLGenerator);
            }
            iLGenerator.MarkLabel(falseLabel);
            iLGenerator.Emit(OpCodes.Stloc, LocalBuilder);
            iLGenerator.Emit(OpCodes.Ldloc, LocalBuilder);
        }

        private void EmitValueInternal(ILGenerator iLGenerator)
        {
            if (Property.DeclaringType.IsNullableType())
            {
                EmitNullablePropertyValue(iLGenerator);
                return;
            }
            iLGenerator.Emit(OpCodes.Callvirt, Property.GetMethod!);
        }

        private void EmitNullablePropertyValue(ILGenerator iLGenerator)
        {
            LocalBuilder? localBuilder;
            //if (Property.Name == "HasValue")
            //{
            localBuilder = iLGenerator.DeclareLocal(Property.DeclaringType);
            iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            iLGenerator.Emit(OpCodes.Ldloca, localBuilder);
            //}
            iLGenerator.Emit(OpCodes.Call, Property.GetMethod!);
        }


        private static bool IsCheckNull(EmitValueDescriptor? descriptor)
        {
            if (descriptor is EmitPropertyDescriptor emitPropertyDescriptor)
            {
                if (!emitPropertyDescriptor.IgnoreNull)
                {
                    return true;
                }
                return IsCheckNull(emitPropertyDescriptor.ParentDescriptor);
            }
            return false;
        }

    }
}
