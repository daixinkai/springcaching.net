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
    internal class CacheEvictAttributeGenerator : AttributeGenerator
    {
        public override bool Build(TypeBuilder typeBuilder, Type attributeType, IList<Attribute> attributes, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            if (attributeType != typeof(CacheEvictAttribute))
            {
                return false;
            }
            var cacheEvictAttributes = attributes.OfType<CacheEvictAttribute>().ToList();



            #region override SpringCachingRequirementProxy.GetCacheEvictRequirements

            var cacheEvictRequirementMethods = DefineCacheEvictRequirementMethods(typeBuilder, cacheEvictAttributes, descriptors);

            var method = typeof(SpringCachingRequirementProxy).GetMethod("GetCacheEvictRequirements")!;
            MethodAttributes methodAttributes =
                MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig
                | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("GetCacheEvictRequirements", methodAttributes, method.ReturnType, Type.EmptyTypes);
            typeBuilder.DefineMethodOverride(methodBuilder, method!);

            var iLGenerator = methodBuilder.GetILGenerator();
            //like new CacheEvictDescriptor[]{x,x,x,x}
            iLGenerator.Emit(OpCodes.Ldc_I4, cacheEvictAttributes.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(ICacheEvictRequirement));
            int index = 0;
            foreach (var cacheEvictRequirementMethod in cacheEvictRequirementMethods)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Call, cacheEvictRequirementMethod);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Ret);
            #endregion

            return true;
        }


        private void GeneratorCacheEvictRequirement(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheEvictAttribute attribute, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            iLGenerator.Emit(OpCodes.Ldstr, attribute.Value);
            iLGenerator.Emit(OpCodes.Newobj, typeof(CacheEvictRequirement).GetConstructorEx());
            //AllEntries
            if (attribute.AllEntries)
            {
                iLGenerator.EmitSetProperty(typeof(CacheEvictRequirement).GetProperty("AllEntries")!, attribute.AllEntries, true);
            }
            //BeforeInvocation
            if (attribute.BeforeInvocation)
            {
                iLGenerator.EmitSetProperty(typeof(CacheEvictRequirement).GetProperty("BeforeInvocation")!, attribute.BeforeInvocation, true);
            }
            SetDefaultProperty(typeBuilder, index, iLGenerator, attribute, descriptors);
        }


        private List<MethodBuilder> DefineCacheEvictRequirementMethods(TypeBuilder typeBuilder, IList<CacheEvictAttribute> cacheEvictAttributes, IList<EmitFieldBuilderDescriptor> descriptors)
        {
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            List<MethodBuilder> methodBuilders = new List<MethodBuilder>();
            int index = 0;
            foreach (var cacheEvictAttribute in cacheEvictAttributes)
            {
                var methodBuilder = typeBuilder.DefineMethod("GetCacheEvictRequirement_" + index, methodAttributes, typeof(ICacheableRequirement), Type.EmptyTypes);
                var iLGenerator = methodBuilder.GetILGenerator();
                GeneratorCacheEvictRequirement(typeBuilder, index, iLGenerator, cacheEvictAttribute, descriptors);
                iLGenerator.Emit(OpCodes.Ret);
                index++;
                methodBuilders.Add(methodBuilder);
            }
            return methodBuilders;
        }

    }
}
