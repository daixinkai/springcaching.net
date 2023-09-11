using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching
{
    partial class ReflectionExtensions
    {
        public static void EmitObjectValue(this ILGenerator iLGenerator, object? value)
        {
            if (value == null)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else if (value is string stringValue)
            {
                iLGenerator.Emit(OpCodes.Ldstr, stringValue);
            }
            else if (value is bool boolValue)
            {
                iLGenerator.EmitBooleanValue(boolValue);
            }
            else if (value is int int32Value)
            {
                iLGenerator.EmitInt32Value(int32Value);
            }
            else if (value is Enum enumValue)
            {
                iLGenerator.EmitInt32Value(enumValue.GetHashCode());
            }
        }

        public static void EmitSetProperty(this ILGenerator iLGenerator, PropertyInfo property, object? value, bool dup)
        {
            if (dup)
            {
                iLGenerator.Emit(OpCodes.Dup);
            }
            iLGenerator.EmitObjectValue(value);
            iLGenerator.Emit(OpCodes.Callvirt, property.SetMethod!);
            if (dup)
            {
                iLGenerator.EmitNop();
            }
        }

        public static void EmitInt32Value(this ILGenerator iLGenerator, int value)
        {
            switch (value)
            {
                case -1:
                    iLGenerator.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    iLGenerator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    iLGenerator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    iLGenerator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    iLGenerator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    iLGenerator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    iLGenerator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    iLGenerator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    iLGenerator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value > -128 && value <= 128)
                    {
                        iLGenerator.Emit(OpCodes.Ldc_I4_S, value);
                    }
                    else
                    {
                        iLGenerator.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }

        }

        public static void EmitBooleanValue(this ILGenerator iLGenerator, bool value)
        {
            if (value)
            {
                iLGenerator.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldc_I4_0);
            }
        }

        public static LocalBuilder? EmitNullablePropertyValue(this ILGenerator iLGenerator, PropertyInfo property)
            => EmitNullableMethod(iLGenerator, property!.GetMethod!);

        public static void EmitNullablePropertyValue(this ILGenerator iLGenerator, PropertyInfo property, ref LocalBuilder? localBuilder)
             => EmitNullableMethod(iLGenerator, property!.GetMethod!, ref localBuilder);

        public static LocalBuilder? EmitNullableMethod(this ILGenerator iLGenerator, MethodInfo method)
        {
            LocalBuilder? localBuilder = null;
            EmitNullableMethod(iLGenerator, method, ref localBuilder);
            return localBuilder;
        }
        public static void EmitNullableMethod(this ILGenerator iLGenerator, MethodInfo method, ref LocalBuilder? localBuilder)
        {
            if (localBuilder == null)
            {
                localBuilder = iLGenerator.DeclareLocal(method.DeclaringType!);
                iLGenerator.Emit(OpCodes.Stloc, localBuilder);
            }
            iLGenerator.Emit(OpCodes.Ldloca, localBuilder);
            iLGenerator.Emit(OpCodes.Call, method);
        }

        public static void EmitLdarg(this ILGenerator iLGenerator, int index)
        {
            switch (index)
            {
                case 0:
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    iLGenerator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    iLGenerator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    iLGenerator.EmitLdargS(index);
                    break;
            }
        }

        public static void EmitLdargS(this ILGenerator iLGenerator, int index)
        {
            if (index > byte.MaxValue)
            {
                iLGenerator.Emit(OpCodes.Ldarg_S, index);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldarg_S, (byte)index);
            }
        }

        public static void EmitNop(this ILGenerator _)
        {
            //#if DEBUG
            //            _.Emit(OpCodes.Nop);
            //#endif
        }

    }
}
