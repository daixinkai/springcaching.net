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
    internal class CachePutAttributeGenerator : AttributeGenerator
    {
        public override bool Build(TypeBuilder typeBuilder, Type attributeType, IList<Attribute> attributes, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            if (attributeType != typeof(CachePutAttribute))
            {
                return false;
            }
            var cachePutAttributes = attributes.OfType<CachePutAttribute>().ToList();

            #region override SpringCachingRequirementProxy.GetCacheEvictRequirements

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
            iLGenerator.Emit(OpCodes.Ldc_I4, cachePutAttributes.Count);
            iLGenerator.Emit(OpCodes.Newarr, typeof(ICachePutRequirement));
            int index = 0;
            foreach (var cachePutAttribute in cachePutAttributes)
            {
                iLGenerator.Emit(OpCodes.Dup);
                iLGenerator.Emit(OpCodes.Ldc_I4, index);
                GeneratorCachePutRequirement(iLGenerator, cachePutAttribute, fieldBuilders);
                iLGenerator.Emit(OpCodes.Stelem_Ref);
                index++;
            }
            iLGenerator.Emit(OpCodes.Ret);
            #endregion

            return true;
        }

        private void GeneratorCachePutRequirement(ILGenerator iLGenerator, CachePutAttribute attribute, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            iLGenerator.Emit(OpCodes.Ldstr, attribute.Value);
            iLGenerator.Emit(OpCodes.Newobj, typeof(CachePutRequirement).GetConstructors()[0]);
            SetDefaultProperty(iLGenerator, attribute, fieldBuilders);
            //Unless
            if (attribute.Unless != null)
            {
                iLGenerator.EmitSetProperty(typeof(CachePutRequirement).GetProperty("Unless")!, attribute.Unless, true);
            }
        }

    }
}
