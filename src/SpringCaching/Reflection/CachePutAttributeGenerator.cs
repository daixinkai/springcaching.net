using SpringCaching.Proxy;
using SpringCaching.Reflection.Emit;
using SpringCaching.Requirement;
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
    internal class CachePutAttributeGenerator : AttributeGenerator
    {
        public override bool Build(TypeBuilder typeBuilder, MethodInfo methodInfo, Type attributeType, IList<Attribute> attributes, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (attributeType != typeof(CachePutAttribute))
            {
                return false;
            }
            var cachePutAttributes = attributes.OfType<CachePutAttribute>().ToList();

            #region override SpringCachingRequirementProxy.GetCacheEvictRequirements

            var cachePutRequirementMethods = DefineCachePutRequirementMethods(typeBuilder, methodInfo, cachePutAttributes, descriptors);

            var method = typeof(SpringCachingRequirementProxy).GetMethod("GetCachePutRequirements")!;
            MethodAttributes methodAttributes =
                MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig
                | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("GetCachePutRequirements", methodAttributes, method.ReturnType, Type.EmptyTypes);
            typeBuilder.DefineMethodOverride(methodBuilder, method!);

            var iLGenerator = methodBuilder.GetILGenerator();
            //like new CachePutDescriptor[]{x,x,x,x}
            iLGenerator.EmitInt32Value(cachePutAttributes.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(ICachePutRequirement));
            int index = 0;
            foreach (var cachePutRequirementMethod in cachePutRequirementMethods)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.EmitInt32Value(index);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Call, cachePutRequirementMethod);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Ret);
            #endregion

            return true;
        }

        private void GeneratorCachePutRequirement(TypeBuilder typeBuilder, MethodInfo methodInfo, int index, ILGenerator iLGenerator, CachePutAttribute attribute, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            iLGenerator.Emit(OpCodes.Ldstr, attribute.Value);
            iLGenerator.Emit(OpCodes.Newobj, typeof(CachePutRequirement).GetConstructorEx());
            //ExpirationPolicy
            iLGenerator.EmitSetProperty(typeof(CachePutRequirement).GetProperty("ExpirationPolicy")!, attribute.ExpirationPolicy, true);
            //ExpirationUnit
            iLGenerator.EmitSetProperty(typeof(CachePutRequirement).GetProperty("ExpirationUnit")!, attribute.ExpirationUnit, true);
            //ExpirationValue
            iLGenerator.EmitSetProperty(typeof(CachePutRequirement).GetProperty("ExpirationValue")!, attribute.ExpirationValue, true);
            //UnlessNull
            if (attribute.UnlessNull)
            {
                iLGenerator.EmitSetProperty(typeof(CachePutRequirement).GetProperty("UnlessNull")!, attribute.UnlessNull, true);
            }
            SetDefaultProperty(typeBuilder, index, iLGenerator, attribute, descriptors);
            SetResultConditionProperty(typeBuilder, methodInfo, index, iLGenerator, attribute, typeof(CacheableRequirement));
        }


        private List<MethodBuilder> DefineCachePutRequirementMethods(TypeBuilder typeBuilder, MethodInfo methodInfo, IList<CachePutAttribute> cachePutAttributes, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            List<MethodBuilder> methodBuilders = new List<MethodBuilder>();
            int index = 0;
            foreach (var cachePutAttribute in cachePutAttributes)
            {
                var methodBuilder = typeBuilder.DefineMethod("GetCachePutRequirement_" + index, methodAttributes, typeof(ICacheableRequirement), Type.EmptyTypes);
                var iLGenerator = methodBuilder.GetILGenerator();
                GeneratorCachePutRequirement(typeBuilder, methodInfo, index, iLGenerator, cachePutAttribute, descriptors);
                iLGenerator.Emit(OpCodes.Ret);
                index++;
                methodBuilders.Add(methodBuilder);
            }
            return methodBuilders;
        }

    }
}
