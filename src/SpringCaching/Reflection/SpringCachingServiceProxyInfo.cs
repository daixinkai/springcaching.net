﻿using SpringCaching.Proxy;
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
    public class SpringCachingServiceProxyInfo : IDisposable
    {
        internal SpringCachingServiceProxyInfo(Type serviceType, DynamicAssembly dynamicAssembly)
        {
            ServiceType = serviceType;
            _dynamicAssembly = dynamicAssembly;
        }
        public Type ServiceType { get; }

        public TypeInfo? TypeInfo { get; private set; }

        public Type? CacheProviderType { get; private set; }

        public Type? CacheProviderFactoryType { get; private set; }

        internal string? Suffix { get; set; }

        private DynamicAssembly _dynamicAssembly;
        private TypeBuilder? _typeBuilder;
        private FieldBuilder? _cacheProviderFieldBuilder;
        private FieldBuilder? _cacheProvideFactoryFieldBuilder;
        private FieldBuilder? _optionsFieldBuilder;

        private bool _implementProxy;

        public void Build()
        {
            if (TypeInfo != null)
            {
                return;
            }

            var springCachingAttribute = ServiceType.GetCustomAttribute<SpringCachingAttribute>();

            if (springCachingAttribute != null)
            {
                SetSpringCachingAttributeData(springCachingAttribute);
            }

            string typeFullName = ServiceType.FullName + "_" + Suffix; ;
            _typeBuilder = CreateTypeBuilder(typeFullName, ServiceType);

            if (!typeof(ISpringCachingProxy).IsAssignableFrom(ServiceType))
            {
                _implementProxy = true;
                _cacheProviderFieldBuilder = _typeBuilder.DefineField("_cacheProvider", CacheProviderType ?? typeof(ICacheProvider), FieldAttributes.Private);
                _optionsFieldBuilder = _typeBuilder.DefineField("_options", typeof(SpringCachingOptions), FieldAttributes.Private);
                if (CacheProviderFactoryType != null)
                {
                    _cacheProvideFactoryFieldBuilder = _typeBuilder.DefineField("_cacheProvideFactory", CacheProviderFactoryType, FieldAttributes.Private);
                }
            }

            if (ServiceType.GetConstructors().Length > 1)
            {
                throw new ArgumentException($"ServiceType : {ServiceType.FullName} has multiple  constructors!");
            }
            var serviceTypeConstructor = ServiceType.GetConstructorEx();
            var serviceTypeConstructorParameters = serviceTypeConstructor.GetParameters();

            var constructorParameterTypes = _implementProxy ?
                serviceTypeConstructorParameters
                .Select(s => s.ParameterType)
                .Concat(new Type[] {
                 _cacheProvideFactoryFieldBuilder?.FieldType ?? CacheProviderType ?? typeof(ICacheProvider),
                    typeof(SpringCachingOptions) }
                ).ToArray()
                : serviceTypeConstructorParameters.Select(s => s.ParameterType).ToArray();

            #region Constructor
            ConstructorBuilder constructorBuilder = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameterTypes);
            int index = 0;

            string cacheProviderParameterName = _cacheProvideFactoryFieldBuilder == null ? "springCachingCacheProvider" : "springCachingCacheProviderFactory";
            string optionsParameterName = "springCachingOptions";
            foreach (var item in serviceTypeConstructorParameters)
            {
                if (cacheProviderParameterName == item.Name)
                {
                    cacheProviderParameterName += "1";
                }
                if (optionsParameterName == item.Name)
                {
                    optionsParameterName += "1";
                }
                index++;
                constructorBuilder.DefineParameter(index, item.Attributes, item.Name);
            }
            if (_implementProxy)
            {
                constructorBuilder.DefineParameter(index + 1, ParameterAttributes.None, cacheProviderParameterName);
                constructorBuilder.DefineParameter(index + 2, ParameterAttributes.None, optionsParameterName);
            }

            ILGenerator constructorIlGenerator = constructorBuilder.GetILGenerator();
            //call base.constructor
            if (serviceTypeConstructorParameters.Length > 0)
            {
                constructorIlGenerator.Emit(OpCodes.Ldarg_0);
                for (int i = 0; i < serviceTypeConstructorParameters.Length; i++)
                {
                    constructorIlGenerator.EmitLdarg(i + 1);
                }
                constructorIlGenerator.Emit(OpCodes.Call, serviceTypeConstructor);
                constructorIlGenerator.EmitNop();
            }
            if (_implementProxy)
            {
                constructorIlGenerator.Emit(OpCodes.Ldarg_0);
                constructorIlGenerator.EmitLdarg(index + 1);
                if (_cacheProvideFactoryFieldBuilder != null)
                {
                    constructorIlGenerator.Emit(OpCodes.Callvirt, typeof(ICacheProviderFactory).GetMethod("GetCacheProvider")!);
                }
                constructorIlGenerator.Emit(OpCodes.Stfld, _cacheProviderFieldBuilder!);
                constructorIlGenerator.Emit(OpCodes.Ldarg_0);
                constructorIlGenerator.EmitLdarg(index + 2);
                constructorIlGenerator.Emit(OpCodes.Stfld, _optionsFieldBuilder!);
            }
            constructorIlGenerator.Emit(OpCodes.Ret);


            #endregion

            #region ISpringCachingProxy
            if (_implementProxy)
            {
                _typeBuilder.AddInterfaceImplementation(typeof(ISpringCachingProxy));
                //ISpringCachingProxy.CacheProvider
                _typeBuilder.DefineExplicitAutoPropertyFromField(typeof(ISpringCachingProxy).GetProperty("CacheProvider")!, _cacheProviderFieldBuilder);
                //ISpringCachingProxy.Options
                _typeBuilder.DefineExplicitAutoPropertyFromField(typeof(ISpringCachingProxy).GetProperty("Options")!, _optionsFieldBuilder);
            }
            #endregion

            foreach (var method in ServiceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(s => s.IsDefined(typeof(CacheBaseAttribute))))
            {
                if (!method.IsVirtual || method.IsFinal)
                {
                    throw new NotSupportedException($"The method \"{method.DeclaringType!.FullName}.{method.Name}({string.Join(",", method.GetParameters().Select(s => s.ParameterType.FullName))})\" must can override");
                    //continue;
                }
                if (method.Attributes.HasFlag(MethodAttributes.Public)
                    || method.Attributes.HasFlag(MethodAttributes.Family)
                    )
                {
                    BuildMethod(method);
                }
            }

            _typeBuilder.CopyCustomAttributes(ServiceType, typeof(SpringCachingAttribute));
            TypeInfo = _typeBuilder.CreateTypeInfo();
        }


        private void BuildMethod(MethodInfo method)
        {

            var baseMethodBuilder = _typeBuilder!.DefineBaseTypeMethod(method);
            MethodAttributes methodAttributes = MethodAttributes.PrivateScope;
            if (method.Attributes.HasFlag(MethodAttributes.Public))
            {
                methodAttributes = MethodAttributes.Public;
            }
            else if (method.Attributes.HasFlag(MethodAttributes.Family))
            {
                methodAttributes = MethodAttributes.Family;
            }
            else if (method.Attributes.HasFlag(MethodAttributes.Assembly))
            {
                methodAttributes = MethodAttributes.Assembly;
            }
            methodAttributes = methodAttributes
                | MethodAttributes.HideBySig
                | MethodAttributes.Virtual;

            var parameters = method.GetParameters();
            Type[] parameterTypes = parameters.Select(s => s.ParameterType).ToArray();
            MethodBuilder methodBuilder = _typeBuilder!.DefineMethod(method.Name, methodAttributes, CallingConventions.Standard, method.ReturnType, parameterTypes);

            #region parameterName

            for (int i = 0; i < parameters.Length; i++)
            {
                methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, parameters[i].Name);
            }

            #endregion

            _typeBuilder!.DefineMethodOverride(methodBuilder, method);
            methodBuilder.CopyCustomAttributes(method);

            ILGenerator iLGenerator = methodBuilder.GetILGenerator();

            var invokeMethod = GetInvokeMethod(method);
            var invokeDelegate = DefineInvokeDelegate(_typeBuilder!, iLGenerator, baseMethodBuilder, parameters, method.GetCustomAttributes<CacheBaseAttribute>().ToArray());

            // new SpringCachingProxyContext(proxy,requirement)
            iLGenerator.Emit(OpCodes.Ldarg_0);  //this
            iLGenerator.Emit(OpCodes.Ldloc, invokeDelegate.Item1);
            iLGenerator.Emit(OpCodes.Newobj, typeof(SpringCachingProxyContext).GetConstructorEx());
            iLGenerator.Emit(OpCodes.Ldloc, invokeDelegate.Item2);
            iLGenerator.Emit(OpCodes.Call, invokeMethod);
            iLGenerator.Emit(OpCodes.Ret);


        }

        private void SetSpringCachingAttributeData(SpringCachingAttribute springCachingAttribute)
        {
            if (springCachingAttribute.CacheProvider != null)
            {
                CheckImpl(typeof(ICacheProvider), springCachingAttribute.CacheProvider);
                CacheProviderType = springCachingAttribute.CacheProvider;
            }
            if (CacheProviderType == null && springCachingAttribute.CacheProviderFactory != null)
            {
                CheckImpl(typeof(ICacheProviderFactory), springCachingAttribute.CacheProviderFactory);
                CacheProviderFactoryType = springCachingAttribute.CacheProviderFactory;
            }
        }

        private void CheckImpl(Type serviceType, Type implType)
        {
            //check 
            if (!serviceType.IsAssignableFrom(implType))
            {
                throw new ArgumentException($"type {implType.FullName} is not implement from {serviceType.Name}");
            }
            if (!implType.IsClass)
            {
                throw new ArgumentException($"type {implType.FullName} is must be a class");
            }
            if (implType.IsAbstract)
            {
                throw new ArgumentException($"type {implType.FullName} can not be abstract");
            }
        }

        /// <summary>
        /// 这里需要生成降级方法委托
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="iLGenerator"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private static Tuple<LocalBuilder, LocalBuilder> DefineInvokeDelegate(TypeBuilder typeBuilder, ILGenerator iLGenerator, MethodBuilder method, ParameterInfo[] parameters, CacheBaseAttribute[] attributes)
        {

            Type delegateType;
            if (method.ReturnType == null || method.ReturnType == typeof(void))
            {
                delegateType = typeof(Action);
            }
            else
            {
                delegateType = typeof(Func<>).MakeGenericType(method.ReturnType);
            }

            //int bindingFlagsValue = 0;
            //foreach (BindingFlags item in Enum.GetValues(typeof(BindingFlags)))
            //{
            //    bindingFlagsValue += item.GetHashCode();
            //}
            //var delegateConstructor = delegateType.GetConstructors((BindingFlags)bindingFlagsValue)[0];

            var delegateConstructor = delegateType.GetConstructorEx();

            LocalBuilder anonymousMethodClass;

            LocalBuilder invokeDelegate = iLGenerator.DeclareLocal(delegateType);
            // if has parameters
            //if (parameters.Length > 0)
            //{
            //方法含有参数的情况下,需要生成一个匿名代理类型
            var anonymousMethodClassTypeBuild = AnonymousMethodClassBuilder.BuildType(typeBuilder, method, parameters, attributes);

            // new anonymousMethodClass

            anonymousMethodClass = iLGenerator.DeclareLocal(anonymousMethodClassTypeBuild.Item1);

            //create type
            anonymousMethodClassTypeBuild.Item1.CreateTypeInfo();

            //field
            iLGenerator.Emit(OpCodes.Ldarg_0); //this
            for (int i = 1; i <= parameters.Length; i++)
            {
                iLGenerator.EmitLdarg(i);
            }

            iLGenerator.Emit(OpCodes.Newobj, anonymousMethodClassTypeBuild.Item2);
            iLGenerator.Emit(OpCodes.Stloc, anonymousMethodClass);
            iLGenerator.Emit(OpCodes.Ldloc, anonymousMethodClass);
            iLGenerator.Emit(OpCodes.Ldftn, anonymousMethodClassTypeBuild.Item3);
            //}
            //else
            //{
            //    iLGenerator.Emit(OpCodes.Ldarg_0); //this
            //    iLGenerator.Emit(OpCodes.Call, method);
            //    iLGenerator.Emit(OpCodes.Dup);
            //    iLGenerator.Emit(OpCodes.Ldvirtftn, method);
            //}

            iLGenerator.Emit(OpCodes.Newobj, delegateConstructor);
            iLGenerator.Emit(OpCodes.Stloc, invokeDelegate);


            return Tuple.Create(anonymousMethodClass, invokeDelegate); ;
        }

        private TypeBuilder CreateTypeBuilder(string typeName, Type? parentType)
        {
            return _dynamicAssembly!.ModuleBuilder.DefineType(typeName,
                          TypeAttributes.Public |
                          TypeAttributes.Class |
                          TypeAttributes.AutoClass |
                          TypeAttributes.AnsiClass |
                          TypeAttributes.BeforeFieldInit |
                          TypeAttributes.AutoLayout,
                          parentType);
        }


        /// <summary>
        /// get the invoke method
        /// <para><see cref="SpringCachingProxyInvoker.Invoke(ISpringCachingProxyContext, Action)"/></para>
        /// <para><see cref="SpringCachingProxyInvoker.Invoke{TResult}(ISpringCachingProxyContext, Func{TResult})"/></para>
        /// <para><see cref="SpringCachingProxyInvoker.InvokeAsync(ISpringCachingProxyContext, Func{Task})"/></para>
        /// <para><see cref="SpringCachingProxyInvoker.InvokeAsync{TResult}(ISpringCachingProxyContext, Func{Task{TResult}})"/></para>
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static MethodInfo GetInvokeMethod(MethodInfo method)
        {
            return GetInvokeMethod(GetReturnType(method), method.IsTaskMethod());
        }
        /// <summary>
        /// get the invoke method
        /// <para><see cref="SpringCachingProxyInvoker.Invoke(ISpringCachingProxyContext, Action)"/></para>
        /// <para><see cref="SpringCachingProxyInvoker.Invoke{TResult}(ISpringCachingProxyContext, Func{TResult})"/></para>
        /// <para><see cref="SpringCachingProxyInvoker.InvokeAsync(ISpringCachingProxyContext, Func{Task})"/></para>
        /// <para><see cref="SpringCachingProxyInvoker.InvokeAsync{TResult}(ISpringCachingProxyContext, Func{Task{TResult}})"/></para>
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="async"></param>
        /// <returns></returns>
        private static MethodInfo GetInvokeMethod(Type returnType, bool async)
        {
            MethodInfo invokeMethod;
            bool isGeneric = !(returnType == null || returnType == typeof(void) || returnType == typeof(Task));
            if (isGeneric)
            {
                invokeMethod = async ? SpringCachingProxyInvoker.GetInvokeAsyncGenericMethod() : SpringCachingProxyInvoker.GetInvokeGenericMethod();
            }
            else
            {
                invokeMethod = async ? SpringCachingProxyInvoker.GetInvokeAsyncMethod() : SpringCachingProxyInvoker.GetInvokeMethod();
            }
            if (isGeneric)
            {
                return invokeMethod.MakeGenericMethod(returnType!);
            }
            return invokeMethod;
        }

        private static Type GetReturnType(MethodInfo method)
        {
            Type returnType;
            if (method.IsTaskMethod() && method.ReturnType.IsGenericType)
            {
                returnType = method.ReturnType.GenericTypeArguments[0];
            }
            else
            {
                returnType = method.ReturnType;
            }
            return returnType;
        }

        public void Dispose()
        {
            _typeBuilder = null;
            _cacheProviderFieldBuilder = null;
            _cacheProvideFactoryFieldBuilder = null;
            _optionsFieldBuilder = null;
        }
    }
}
