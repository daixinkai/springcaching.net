using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using SpringCaching.Proxy;
using System.Runtime.InteropServices;
using SpringCaching.Reflection.Emit;
using System.Linq;

namespace SpringCaching.Reflection
{
    internal static class AnonymousMethodClassBuilder
    {
        static AnonymousMethodClassBuilder()
        {
            s_attributeGenerators = new List<AttributeGenerator>();
            s_attributeGenerators.Add(new CacheableAttributeGenerator());
            s_attributeGenerators.Add(new CacheEvictAttributeGenerator());
            s_attributeGenerators.Add(new CachePutAttributeGenerator());
        }
        private static readonly List<AttributeGenerator> s_attributeGenerators;

        public static Tuple<TypeBuilder, ConstructorInfo, MethodInfo> BuildType(ModuleBuilder moduleBuilder, MethodInfo method, ParameterInfo[] parameters, CacheBaseAttribute[] attributes)
        {
#if DEBUG && NET45
            string _suffix = method.Name + "_" + string.Join("_", parameters.Select(s => s.ParameterType.Name));
#else
            string _suffix = Guid.NewGuid().ToString("N").ToUpper();
#endif
            string typeName = method.DeclaringType!.Name;
            typeName += "_" + _suffix;
            string nameSpace = method.DeclaringType.Namespace + ".Anonymous";
            string fullName = nameSpace + "." + typeName;
            if (fullName.StartsWith("."))
            {
                fullName = fullName.TrimStart('.');
            }
            TypeBuilder typeBuilder = CreateTypeBuilder(moduleBuilder, fullName, typeof(SpringCachingRequirementProxy));
            return FillType(typeBuilder, method, parameters, attributes);
        }


        public static Tuple<TypeBuilder, ConstructorInfo, MethodInfo> BuildType(TypeBuilder nestedForTypeBuilder, MethodInfo method, ParameterInfo[] parameters, CacheBaseAttribute[] attributes)
        {
#if DEBUG && NET45
            string _suffix = method.Name + "_" + string.Join("_", parameters.Select(s => s.ParameterType.Name));
#else
            string _suffix = Guid.NewGuid().ToString("N").ToUpper();
#endif
            string fullName = nestedForTypeBuilder.BaseType!.Name + "_" + _suffix;
            TypeBuilder typeBuilder = CreateTypeBuilder(nestedForTypeBuilder, fullName, typeof(SpringCachingRequirementProxy));
            return FillType(typeBuilder, method, parameters, attributes);
        }


        private static FieldBuilder CreateField(TypeBuilder typeBuilder, string fieldName, Type fieldType)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Private);
            return fieldBuilder;
        }

        private static ConstructorBuilder CreateConstructor(TypeBuilder typeBuilder, List<FieldBuilder> fieldBuilders)
        {
            List<Type> types = fieldBuilders.Select(s => s.FieldType).ToList();
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
               MethodAttributes.Public,
               CallingConventions.Standard,
               fieldBuilders.Select(s => s.FieldType).ToArray());

            for (int i = 0; i < fieldBuilders.Count; i++)
            {
                constructorBuilder.DefineParameter(i + 1, ParameterAttributes.None, fieldBuilders[i].Name);
            }

            ILGenerator constructorIlGenerator = constructorBuilder.GetILGenerator();

            CallBaseTypeDefaultConstructor(constructorIlGenerator, typeBuilder.BaseType!);
            constructorIlGenerator.Emit(OpCodes.Nop);
            if (fieldBuilders.Count > 0)
            {
                for (int i = 0; i < fieldBuilders.Count; i++)
                {
                    constructorIlGenerator.Emit(OpCodes.Ldarg_0);
                    constructorIlGenerator.Emit(OpCodes.Ldarg_S, (i + 1));
                    constructorIlGenerator.Emit(OpCodes.Stfld, fieldBuilders[i]);
                }
            }
            constructorIlGenerator.Emit(OpCodes.Ret);
            return constructorBuilder;
        }

        private static MethodBuilder CreateMethod(TypeBuilder typeBuilder, MethodInfo method, List<FieldBuilder> fieldBuilders)
        {

            //MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;
            MethodAttributes methodAttributes = MethodAttributes.Assembly | MethodAttributes.HideBySig;
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name, methodAttributes, CallingConventions.Standard, method.ReturnType, Type.EmptyTypes);
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();
            if (fieldBuilders.Count > 0)
            {
                for (int i = 0; i < fieldBuilders.Count; i++)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0); // this
                    iLGenerator.Emit(OpCodes.Ldfld, fieldBuilders[i]);
                }
            }

            iLGenerator.Emit(OpCodes.Call, method);
            iLGenerator.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private static Tuple<TypeBuilder, ConstructorInfo, MethodInfo> FillType(TypeBuilder typeBuilder, MethodInfo method, ParameterInfo[] parameters, CacheBaseAttribute[] attributes)
        {
            // field
            parameters ??= method.GetParameters();
            var fieldBuilders = new List<FieldBuilder>();
            if (!method!.IsStatic)
            {
                if (method.DeclaringType == null)
                {
                    throw new ArgumentException("targetType");
                }
                fieldBuilders.Add(CreateField(typeBuilder, "_this_Service", method.DeclaringType));
            }
            var fieldBuilderDescribes = new List<EmitFieldBuilderDescriptor>();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var fieldBuilderDescribe = new EmitFieldBuilderDescriptor(
                   parameter,
                    CreateField(typeBuilder, "_" + parameter.Name, parameter.ParameterType)
                    );
                fieldBuilderDescribes.Add(fieldBuilderDescribe);
                fieldBuilders.Add(fieldBuilderDescribe.FieldBuilder);
            }

            //constructor
            ConstructorBuilder constructorBuilder = CreateConstructor(typeBuilder, fieldBuilders);

            //arguments

            AddArgumentsProperty(typeBuilder, fieldBuilderDescribes);

            ////DefaultNullValue
            //typeBuilder.OverrideProperty(typeof(SpringCachingRequirementProxy).GetProperty("DefaultNullValue")!, iLGenerator =>
            //{
            //    iLGenerator.EmitObjectValue(true);
            //    iLGenerator.Emit(OpCodes.Ret);
            //}, null);

            //attributes

            if (attributes != null && attributes.Length > 0)
            {

                var cacheableAttributes = attributes.Where(s => typeof(CacheableAttribute).IsAssignableFrom(s.GetType())).ToList<Attribute>();
                var cacheEvictAttributes = attributes.Where(s => typeof(CacheEvictAttribute).IsAssignableFrom(s.GetType())).ToList<Attribute>();
                var cachePutAttributes = attributes.Where(s => typeof(CachePutAttribute).IsAssignableFrom(s.GetType())).ToList<Attribute>();

                if (cacheableAttributes.Count > 0)
                {
                    foreach (var attributeGenerator in s_attributeGenerators)
                    {
                        if (attributeGenerator.Build(typeBuilder, typeof(CacheableAttribute), cacheableAttributes, fieldBuilderDescribes))
                        {
                            continue;
                        }
                    }
                }

                if (cacheEvictAttributes.Count > 0)
                {
                    foreach (var attributeGenerator in s_attributeGenerators)
                    {
                        if (attributeGenerator.Build(typeBuilder, typeof(CacheEvictAttribute), cacheEvictAttributes, fieldBuilderDescribes))
                        {
                            continue;
                        }
                    }
                }

                if (cachePutAttributes.Count > 0)
                {
                    foreach (var attributeGenerator in s_attributeGenerators)
                    {
                        if (attributeGenerator.Build(typeBuilder, typeof(CachePutAttribute), cachePutAttributes, fieldBuilderDescribes))
                        {
                            continue;
                        }
                    }
                }

                //foreach (var group in attributes.GroupBy(s => s.GetType()))
                //{
                //    foreach (var attributeGenerator in s_attributeGenerators)
                //    {
                //        if (attributeGenerator.Build(typeBuilder, group.Key, group.ToList<Attribute>(), fieldBuilderDescribes))
                //        {
                //            continue;
                //        }
                //    }
                //}

            }

            MethodBuilder methodBuilder = CreateMethod(typeBuilder, method, fieldBuilders);

            return Tuple.Create(typeBuilder, (ConstructorInfo)constructorBuilder, (MethodInfo)methodBuilder);
        }

        private static TypeBuilder CreateTypeBuilder(ModuleBuilder moduleBuilder, string typeName, Type? parentType)
        {
            return moduleBuilder.DefineType(typeName,
                          //TypeAttributes.Public |
                          TypeAttributes.NotPublic |
                          TypeAttributes.Class |
                          TypeAttributes.AutoClass |
                          TypeAttributes.AnsiClass |
                          TypeAttributes.BeforeFieldInit |
                          TypeAttributes.AutoLayout |
                          TypeAttributes.Sealed,
                          parentType);
        }

        private static TypeBuilder CreateTypeBuilder(TypeBuilder nestedForTypeBuilder, string typeName, Type? parentType)
        {
            return nestedForTypeBuilder.DefineNestedType(typeName,
                          TypeAttributes.NestedPrivate |
                          TypeAttributes.Class |
                          TypeAttributes.AutoClass |
                          TypeAttributes.AnsiClass |
                          TypeAttributes.BeforeFieldInit |
                          TypeAttributes.AutoLayout |
                           TypeAttributes.Sealed,
                          parentType);
        }


        private static void CallBaseTypeDefaultConstructor(ILGenerator constructorIlGenerator, Type baseType)
        {
            var defaultConstructor = baseType.GetConstructorEx(Type.EmptyTypes);
            if (defaultConstructor == null)
            {
                throw new ArgumentException("The default constructor not found . Type : " + baseType.FullName);
            }
            constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            constructorIlGenerator.Emit(OpCodes.Call, defaultConstructor);
        }

        private static void AddArgumentsProperty(TypeBuilder typeBuilder, List<EmitFieldBuilderDescriptor> descriptors)
        {
            var property = typeof(SpringCachingRequirementProxy).GetProperty("Arguments")!;
            typeBuilder.OverrideProperty(property, iLGenerator =>
            {
                LocalBuilder map = iLGenerator.DeclareLocal(typeof(IDictionary<string, object>));
                iLGenerator.Emit(OpCodes.Newobj, typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes)!);
                iLGenerator.Emit(OpCodes.Stloc, map);
                //iLGenerator.Emit(OpCodes.Pop);
                MethodInfo addMethod = typeof(IDictionary<string, object>).GetMethod("Add", new Type[] { typeof(string), typeof(object) })!;

                for (int i = 0; i < descriptors.Count; i++)
                {
                    var descriptor = descriptors[i];
                    iLGenerator.Emit(OpCodes.Ldloc, map);
                    iLGenerator.Emit(OpCodes.Ldstr, descriptor.Parameter.Name!);
                    descriptor.EmitValue(iLGenerator);
                    descriptor.EmitBox(iLGenerator);
                    iLGenerator.Emit(OpCodes.Callvirt, addMethod);
                }

                iLGenerator.Emit(OpCodes.Ldloc, map);
                iLGenerator.Emit(OpCodes.Ret);
            }, null);
        }


    }
}
