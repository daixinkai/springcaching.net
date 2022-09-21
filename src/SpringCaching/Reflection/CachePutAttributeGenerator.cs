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
            if (attributeType != typeof(CacheEvictAttribute))
            {
                return false;
            }
            //var cacheEvictAttribute = attribute as ICacheEvictRequirement;
            //if (cacheEvictAttribute == null)
            //{
            //    return false;
            //}
            //typeBuilder.AddInterfaceImplementation(typeof(ICacheEvictRequirement));
            //AddExplicitAutoProperty(typeBuilder, cacheEvictAttribute);
            return false;
            //var cachePutAttribute = attribute as ICachePutRequirement;
            //if (cachePutAttribute == null)
            //{
            //    return false;
            //}
            //typeBuilder.AddInterfaceImplementation(typeof(ICachePutRequirement));
            //AddExplicitAutoProperty(typeBuilder, cachePutAttribute);
            //return true;
        }

        private void GeneratorCachePutRequirement(ILGenerator iLGenerator, CachePutRequirement attribute, IList<FieldBuilderDescriptor> fieldBuilders)
        {
            iLGenerator.Emit(OpCodes.Ldstr, attribute.Value);
            EmitKeyGenerator(iLGenerator, attribute.Key, fieldBuilders);
            iLGenerator.Emit(OpCodes.Newobj, typeof(CachePutRequirement).GetConstructors()[0]);
            #region other property
            //Condition
            if (attribute.Condition != null)
            {
                iLGenerator.EmitSetProperty(typeof(CachePutRequirement).GetProperty("Condition"), attribute.Condition, true);
            }
            #endregion
        }

    }
}
