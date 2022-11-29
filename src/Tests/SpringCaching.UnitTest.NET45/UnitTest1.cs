using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpringCaching.Infrastructure;
using SpringCaching.Reflection;
using SpringCaching.Tests;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SpringCaching.UnitTest.NET45
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            //var ss = new SimpleKeyGenerators.StructToStringKeyGenerator<int>(1);
            DynamicAssembly dynamicAssembly = SpringCachingServiceProxy.DynamicAssembly;
            dynamicAssembly.DEBUG_MODE = true;
            var testServiceTypeInfo = SpringCachingServiceProxy.GetServiceProxyInfo(typeof(TestService));
            dynamicAssembly.AssemblyBuilder.Save(dynamicAssembly.AssemblyName);
            var testServiceType = testServiceTypeInfo.TypeInfo.AsType();
            object[] param = new object[2];
            param[1] = new SpringCachingOptions();
            if (testServiceTypeInfo.CacheProviderType != null)
            {
                param[0] = Activator.CreateInstance(testServiceTypeInfo.CacheProviderType);
            }
            else if (testServiceTypeInfo.CacheProviderFactoryType != null)
            {
                param[0] = Activator.CreateInstance(testServiceTypeInfo.CacheProviderFactoryType);
            }
            else
            {
                param[0] = new EmptyCacheProvider();
            }
            ITestService testService = Activator.CreateInstance(testServiceType, param) as ITestService;
            var names = await testService.GetNames(1);
            Assert.IsNotNull(names);
        }


        [TestMethod]
        public async Task TestMethod2()
        {
            DynamicAssembly dynamicAssembly = SpringCachingServiceProxy.DynamicAssembly;
            dynamicAssembly.DEBUG_MODE = true;
            var testServiceTypeInfo = SpringCachingServiceProxy.GetServiceProxyInfo(typeof(TestUserService));
            dynamicAssembly.AssemblyBuilder.Save(dynamicAssembly.AssemblyName);
            var testServiceType = testServiceTypeInfo.TypeInfo.AsType();
            object[] param = new object[2];
            param[1] = new SpringCachingOptions();
            if (testServiceTypeInfo.CacheProviderType != null)
            {
                param[0] = Activator.CreateInstance(testServiceTypeInfo.CacheProviderType);
            }
            else if (testServiceTypeInfo.CacheProviderFactoryType != null)
            {
                param[0] = Activator.CreateInstance(testServiceTypeInfo.CacheProviderFactoryType);
            }
            else
            {
                param[0] = new EmptyCacheProvider();
            }
            TestUserService testService = Activator.CreateInstance(testServiceType, param) as TestUserService;
            var user = await testService.GetUserAsync(1);
            Assert.IsNotNull(user);
        }
        //private static Expression CreateRecursiveExpression()
        //{
        //    var methodInfo = typeof(Console).GetMethod("WriteLine", new[] { typeof(String) });
        //    var arg = Expression.Constant("Hello");
        //    var consoleCall = Expression.Call(methodInfo, arg);
        //    var sayHelloActionVariable = Expression.Variable(typeof(Action), "sayHelloAction");
        //    var block = Expression.Block(
        //        new[] { sayHelloActionVariable },
        //        Expression.Assign(
        //            sayHelloActionVariable,
        //            Expression.Lambda(
        //                Expression.Block(
        //                    consoleCall,
        //                    Expression.Invoke(sayHelloActionVariable)
        //                )
        //            )
        //        ),
        //        Expression.Invoke(sayHelloActionVariable)
        //    );

        //    return block;
        //}


    }
}
