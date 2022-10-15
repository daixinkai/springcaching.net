using SpringCaching.Proxy;
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

            var cacheableRequirementMethods = DefineCacheableRequirementMethods(typeBuilder, cacheableAttributes, fieldBuilders);

            var method = typeof(SpringCachingRequirementProxy).GetMethod("GetCacheableRequirements")!;

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
            foreach (var cacheableRequirementMethod in cacheableRequirementMethods)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Call, cacheableRequirementMethod);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Ret);
            #endregion
            return true;
        }


        private void GeneratorCacheableRequirement(TypeBuilder typeBuilder, int index, ILGenerator iLGenerator, CacheableAttribute attribute, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            iLGenerator.Emit(OpCodes.Ldstr, attribute.Value);
            iLGenerator.Emit(OpCodes.Newobj, typeof(CacheableRequirement).GetConstructors()[0]);
            //ExpirationPolicy
            iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("ExpirationPolicy")!, attribute.ExpirationPolicy, true);
            //ExpirationUnit
            iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("ExpirationUnit")!, attribute.ExpirationUnit, true);
            //ExpirationValue
            iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("ExpirationValue")!, attribute.ExpirationValue, true);
            //UnlessNull
            if (attribute.UnlessNull)
            {
                iLGenerator.EmitSetProperty(typeof(CacheableRequirement).GetProperty("UnlessNull")!, attribute.UnlessNull, true);
            }
            SetDefaultProperty(typeBuilder, index, iLGenerator, attribute, fieldBuilders);
        }

        private List<MethodBuilder> DefineCacheableRequirementMethods(TypeBuilder typeBuilder, IList<CacheableAttribute> cacheableAttributes, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            MethodAttributes methodAttributes =
                MethodAttributes.Private
                | MethodAttributes.HideBySig;
            List<MethodBuilder> methodBuilders = new List<MethodBuilder>();
            int index = 0;
            foreach (var cacheableAttribute in cacheableAttributes)
            {
                var methodBuilder = typeBuilder.DefineMethod("GetCacheableRequirement_" + index, methodAttributes, typeof(ICacheableRequirement), Type.EmptyTypes);
                var iLGenerator = methodBuilder.GetILGenerator();
                GeneratorCacheableRequirement(typeBuilder, index, iLGenerator, cacheableAttribute, fieldBuilders);
                iLGenerator.Emit(OpCodes.Ret);
                index++;
                methodBuilders.Add(methodBuilder);
            }
            return methodBuilders;
        }

    }

}
