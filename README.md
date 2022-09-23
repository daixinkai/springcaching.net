# springcaching.net

*spring Cacheable,CacheEvict,CachePut for .net*

## Usage

1. Install the NuGet package

    `PM> Install-Package SpringCaching`

2. In your service method
   
   ```c#
    [SpringCaching]
    public class TestService : ITestService
    {
        [Cacheable("getAllTest", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        [CachePut("getAllTest_CachePut", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetAll()
        {
            // method must be virtual
            return Task.FromResult(new List<string>());
        }
        [Cacheable("getNames_Param", Key = "#param.Id", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(TestServiceParam param)
        {
            return Task.FromResult(new List<string>() { param.Id.ToString(), param.Name });
        }
        [Cacheable("getTest", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        [CachePut("getTest_CachePut", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        protected virtual Task<List<string>> Get(int id)
        {
            // support protected method
            return Task.FromResult(new List<string>());
        }
        [CacheEvict("getAllTest")]
        protected virtual Task Update()
        {
            return Task.CompletedTask;
        }     
        [CacheEvict("getTest")]
        public virtual Task Update(int id)
        {
            return Task.CompletedTask;
        }                
    }   
   ```

3. Install the NuGet package on .net core application

    `PM> Install-Package SpringCaching.DependencyInjection`

    ```c#
        services.AddTransient<interface,impl > ();
        services.AddScoped <interface,impl > ();
        services.AddSingleton <interface,impl>();
        .....................
        //at last
        services.AddSpringCaching();
    ```
