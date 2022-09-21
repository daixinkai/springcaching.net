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

    internal class CacheableAttributeGenerator : AttributeGenerator
    {
        public override bool Build(TypeBuilder typeBuilder, Type attributeType, IList<Attribute> attributes, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            if (attributeType != typeof(CacheableAttribute))
            {
                return false;
            }
            //foreach (var item in attributes.Cast<CacheableAttribute>())
            //{
            //    typeBuilder.AddInterfaceImplementation(typeof(ICacheableRequirement));
            //    AddExplicitAutoProperty(typeBuilder, (ICacheableRequirement)item);

            //}

            var cacheableAttributes = attributes.OfType<CacheableAttribute>().ToList();

            #region override SpringCachingRequirementProxy.GetCacheableRequirements

            var method = typeof(SpringCachingRequirementProxy).GetMethod("GetCacheableRequirements");

            MethodAttributes methodAttributes =
                MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig
                | MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod("GetCacheableRequirements", methodAttributes, method.ReturnType, Type.EmptyTypes);
            typeBuilder.DefineMethodOverride(methodBuilder, method!);

            var iLGenerator = methodBuilder.GetILGenerator();
            //like new CacheCacheableDescriptor[]{x,x,x,x}
            iLGenerator.Emit(OpCodes.Ldc_I4, cacheableAttributes.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(ICacheableRequirement));
            int index = 0;
            foreach (var cacheableAttribute in cacheableAttributes)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                GeneratorCacheableRequirement(iLGenerator, cacheableAttribute, fieldBuilders);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Ret);
            #endregion
            return true;
        }


        private void GeneratorCacheableRequirement(ILGenerator iLGenerator, CacheableAttribute attribute, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            iLGenerator.Emit(OpCodes.Ldstr, attribute.Value);
            EmitKeyGenerator(iLGenerator, attribute.Key, fieldBuilders);
            iLGenerator.Emit(OpCodes.Newobj, typeof(CacheableRequirement).GetConstructors()[0]);
            #region other property
            //ExpirationPolicy
            iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("ExpirationPolicy"), attribute.ExpirationPolicy, true);
            //ExpirationUnit
            iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("ExpirationUnit"), attribute.ExpirationUnit, true);
            //ExpirationValue
            iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("ExpirationValue"), attribute.ExpirationValue, true);
            //Condition
            if (attribute.Condition != null)
            {
                iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("Condition"), attribute.Condition, true);
            }
            #endregion
        }

    }

}
