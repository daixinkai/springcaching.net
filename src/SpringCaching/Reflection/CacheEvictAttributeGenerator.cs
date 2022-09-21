using SpringCaching.Proxy;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
    internal class CacheEvictAttributeGenerator : AttributeGenerator
    {
        public override bool Build(TypeBuilder typeBuilder, Type attributeType, IList<Attribute> attributes, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            if (attributeType != typeof(CacheEvictAttribute))
            {
                return false;
            }
            var cacheEvictAttributes = attributes.OfType<CacheEvictAttribute>().ToList();

            #region override SpringCachingRequirementProxy.GetCacheEvictRequirements

            var method = typeof(SpringCachingRequirementProxy).GetMethod("GetCacheEvictRequirements");
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
            foreach (var cacheEvictAttribute in cacheEvictAttributes)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                GeneratorCacheEvictRequirement(iLGenerator, cacheEvictAttribute, fieldBuilders);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Ret);
            #endregion

            return true;
        }


        private void GeneratorCacheEvictRequirement(ILGenerator iLGenerator, CacheEvictAttribute attribute, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            iLGenerator.Emit(OpCodes.Ldstr, attribute.Value);
            EmitKeyGenerator(iLGenerator, attribute.Key, fieldBuilders);
            iLGenerator.Emit(OpCodes.Newobj, typeof(CacheEvictRequirement).GetConstructors()[0]);
            #region other property
            //Condition
            if (attribute.Condition != null)
            {
                iLGenerator.EmitSetProperty(typeof(CacheEvictRequirement).GetProperty("Condition"), attribute.Condition, true);
            }
            #endregion
        }

    }
}
