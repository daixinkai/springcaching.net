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
        [Cacheable("getNames_Param", Key = "#param.Id", Condition = "#param.Id>=0&&#param.Name!=null", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<List<string>> GetNames(TestServiceParam param)
        {
            return Task.FromResult(new List<string>() { param.Id.ToString(), param.Name });
        }
        [Cacheable("getTest", Condition = "#id>0", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        [CachePut("getTest_CachePut", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        protected virtual Task<List<string>> Get(int id)
        {
            // support protected method
            return Task.FromResult(new List<string>());
        }
        [CacheEvict("getAllTest", AllEntries = true)]
        protected virtual Task Update()
        {
            return Task.CompletedTask;
        }     
        [CacheEvict("getTest", Condition = "#id>0")]
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

## KeyExpression

- [ ] #param.xxxx()  --  not support method
- [x] #id
- [x] #param.Id
- [x] #param.Id + #param.Name
- [x] #param.Id + '_' + #param.Name

## ConditionExpression

- [ ] #param.xxxx()  --  not support method
- [x] #id > 0
- [x] !#param.Id.HasValue
- [x] #id > 0 && #param.Id.HasValue
- [x] !(#id > 0 && !#param.Id.HasValue)
- [ ] !(#id > 0 || !#param.Id.HasValue)
- [x] !(#id > 0 && !#param.Id.HasValue) || #param.Count > 0


## EmitType


<details>
<summary>TestUserService</summary>

```csharp
    [SpringCaching]
    public class TestUserService
    {
        [Cacheable("users", Key = "#id", Condition = "#id>0", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1)]
        public virtual Task<UserResultDto> GetUserAsync(int id)
        {
            return Task.FromResult(new UserResultDto
            {
                Id = id
            });
        }

        [CacheEvict("users", Key = "#user.Id", Condition = "#user!=null&&#user.Id>0")]
        public virtual Task UpdateUserAsync(UserResultDto user)
        {
            return Task.FromResult(0);
        }

        [CacheEvict("users", AllEntries = true)]
        public virtual Task ClearCache()
        {
            return Task.FromResult(0);
        }

    }
```
</details>

<details>
<summary>TestUserServiceProxy</summary>

```csharp
  [StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
	public class TestUserService_BB93BE8338034C5FBC946FB266372676 : TestUserService, ISpringCachingProxy
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002378 File Offset: 0x00000578
		public TestUserService_BB93BE8338034C5FBC946FB266372676(ICacheProvider springCachingCacheProvider, SpringCachingOptions springCachingOptions)
		{
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x0000239C File Offset: 0x0000059C
		ICacheProvider ISpringCachingProxy.CacheProvider
		{
			get
			{
				return this._cacheProvider;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000003 RID: 3 RVA: 0x000023B0 File Offset: 0x000005B0
		SpringCachingOptions ISpringCachingProxy.Options
		{
			get
			{
				return this._options;
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000023C4 File Offset: 0x000005C4
		[CompilerGenerated]
		private Task<UserResultDto> GetUserAsync<>n__(int id)
		{
			return base.GetUserAsync(id);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000023DC File Offset: 0x000005DC
		[Cacheable("users", ExpirationPolicy = ExpirationPolicy.Absolute, ExpirationUnit = ExpirationUnit.Minute, ExpirationValue = 1, Key = "#id", Condition = "#id>0")]
		public override Task<UserResultDto> GetUserAsync(int id)
		{
			TestUserService_BB93BE8338034C5FBC946FB266372676.TestUserService_GetUserAsync<>n___Int32 testUserService_GetUserAsync<>n___Int = new TestUserService_BB93BE8338034C5FBC946FB266372676.TestUserService_GetUserAsync<>n___Int32(this, id);
			Func<Task<UserResultDto>> invoker = new Func<Task<UserResultDto>>(testUserService_GetUserAsync<>n___Int.GetUserAsync<>n__);
			return SpringCachingProxyInvoker.InvokeAsync<UserResultDto>(new SpringCachingProxyContext(this, testUserService_GetUserAsync<>n___Int), invoker);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002410 File Offset: 0x00000610
		[CompilerGenerated]
		private Task UpdateUserAsync<>n__(UserResultDto user)
		{
			return base.UpdateUserAsync(user);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002428 File Offset: 0x00000628
		[CacheEvict("users", Key = "#user.Id", Condition = "#user!=null&&#user.Id>0")]
		public override Task UpdateUserAsync(UserResultDto user)
		{
			TestUserService_BB93BE8338034C5FBC946FB266372676.TestUserService_UpdateUserAsync<>n___UserResultDto testUserService_UpdateUserAsync<>n___UserResultDto = new TestUserService_BB93BE8338034C5FBC946FB266372676.TestUserService_UpdateUserAsync<>n___UserResultDto(this, user);
			Func<Task> invoker = new Func<Task>(testUserService_UpdateUserAsync<>n___UserResultDto.UpdateUserAsync<>n__);
			return SpringCachingProxyInvoker.InvokeAsync(new SpringCachingProxyContext(this, testUserService_UpdateUserAsync<>n___UserResultDto), invoker);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000245C File Offset: 0x0000065C
		[CompilerGenerated]
		private Task ClearCache<>n__()
		{
			return base.ClearCache();
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002470 File Offset: 0x00000670
		[CacheEvict("users", AllEntries = true)]
		public override Task ClearCache()
		{
			TestUserService_BB93BE8338034C5FBC946FB266372676.TestUserService_ClearCache<>n___ testUserService_ClearCache<>n___ = new TestUserService_BB93BE8338034C5FBC946FB266372676.TestUserService_ClearCache<>n___(this);
			Func<Task> invoker = new Func<Task>(testUserService_ClearCache<>n___.ClearCache<>n__);
			return SpringCachingProxyInvoker.InvokeAsync(new SpringCachingProxyContext(this, testUserService_ClearCache<>n___), invoker);
		}

		// Token: 0x04000001 RID: 1
		private ICacheProvider _cacheProvider = springCachingCacheProvider;

		// Token: 0x04000002 RID: 2
		private SpringCachingOptions _options = springCachingOptions;

		// Token: 0x02000003 RID: 3
		[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
		private sealed class TestUserService_GetUserAsync<>n___Int32 : SpringCachingRequirementProxy
		{
			// Token: 0x0600000A RID: 10 RVA: 0x00002050 File Offset: 0x00000250
			public TestUserService_GetUserAsync<>n___Int32(TestUserService_BB93BE8338034C5FBC946FB266372676 _this_Service, int _id)
			{
				this._this_Service = _this_Service;
				this._id = _id;
			}

			// Token: 0x17000003 RID: 3
			// (get) Token: 0x0600000B RID: 11 RVA: 0x0000207C File Offset: 0x0000027C
			[Nullable(new byte[]
			{
				2,
				1,
				1
			})]
			public override IDictionary<string, object> Arguments
			{
				get
				{
					return new Dictionary<string, object>
					{
						{
							"id",
							this._id
						}
					};
				}
			}

			// Token: 0x0600000C RID: 12 RVA: 0x000020A8 File Offset: 0x000002A8
			private ICacheableRequirement GetCacheableRequirement_0()
			{
				return new CacheableRequirement("users")
				{
					ExpirationPolicy = ExpirationPolicy.Absolute,
					ExpirationUnit = ExpirationUnit.Minute,
					ExpirationValue = 1,
					Key = "#id",
					Condition = "#id>0",
					KeyGenerator = new FuncKeyGenerator(new Func<string>(this.GetCacheableKey_0)),
					ConditionGenerator = new FuncPredicateGenerator(new Func<bool>(this.GetCacheableCondition_0))
				};
			}

			// Token: 0x0600000D RID: 13 RVA: 0x00002120 File Offset: 0x00000320
			private string GetCacheableKey_0()
			{
				return base.ToStringFromStruct<int>(this._id);
			}

			// Token: 0x0600000E RID: 14 RVA: 0x0000213C File Offset: 0x0000033C
			private bool GetCacheableCondition_0()
			{
				return this._id > 0;
			}

			// Token: 0x0600000F RID: 15 RVA: 0x00002158 File Offset: 0x00000358
			public override IList<ICacheableRequirement> GetCacheableRequirements()
			{
				return new ICacheableRequirement[]
				{
					this.GetCacheableRequirement_0()
				};
			}

			// Token: 0x06000010 RID: 16 RVA: 0x0000217C File Offset: 0x0000037C
			internal Task<UserResultDto> GetUserAsync<>n__()
			{
				return this._this_Service.GetUserAsync<>n__(this._id);
			}

			// Token: 0x04000003 RID: 3
			private TestUserService_BB93BE8338034C5FBC946FB266372676 _this_Service;

			// Token: 0x04000004 RID: 4
			private int _id;
		}

		// Token: 0x02000004 RID: 4
		[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
		private sealed class TestUserService_UpdateUserAsync<>n___UserResultDto : SpringCachingRequirementProxy
		{
			// Token: 0x06000011 RID: 17 RVA: 0x0000219C File Offset: 0x0000039C
			public TestUserService_UpdateUserAsync<>n___UserResultDto(TestUserService_BB93BE8338034C5FBC946FB266372676 _this_Service, UserResultDto _user)
			{
				this._this_Service = _this_Service;
				this._user = _user;
			}

			// Token: 0x17000004 RID: 4
			// (get) Token: 0x06000012 RID: 18 RVA: 0x000021C8 File Offset: 0x000003C8
			[Nullable(new byte[]
			{
				2,
				1,
				1
			})]
			public override IDictionary<string, object> Arguments
			{
				get
				{
					return new Dictionary<string, object>
					{
						{
							"user",
							this._user
						}
					};
				}
			}

			// Token: 0x06000013 RID: 19 RVA: 0x000021F0 File Offset: 0x000003F0
			private ICacheableRequirement GetCacheEvictRequirement_0()
			{
				return new CacheEvictRequirement("users")
				{
					Key = "#user.Id",
					Condition = "#user!=null&&#user.Id>0",
					KeyGenerator = new FuncKeyGenerator(new Func<string>(this.GetCacheEvictKey_0)),
					ConditionGenerator = new FuncPredicateGenerator(new Func<bool>(this.GetCacheEvictCondition_0))
				};
			}

			// Token: 0x06000014 RID: 20 RVA: 0x00002250 File Offset: 0x00000450
			private string GetCacheEvictKey_0()
			{
				return base.ToStringFromStruct<int>(this._user.Id);
			}

			// Token: 0x06000015 RID: 21 RVA: 0x00002270 File Offset: 0x00000470
			private bool GetCacheEvictCondition_0()
			{
				return this._user != null && this._user.Id > 0;
			}

			// Token: 0x06000016 RID: 22 RVA: 0x000022A4 File Offset: 0x000004A4
			public override IList<ICacheEvictRequirement> GetCacheEvictRequirements()
			{
				return new ICacheEvictRequirement[]
				{
					this.GetCacheEvictRequirement_0()
				};
			}

			// Token: 0x06000017 RID: 23 RVA: 0x000022C8 File Offset: 0x000004C8
			internal Task UpdateUserAsync<>n__()
			{
				return this._this_Service.UpdateUserAsync<>n__(this._user);
			}

			// Token: 0x04000005 RID: 5
			private TestUserService_BB93BE8338034C5FBC946FB266372676 _this_Service;

			// Token: 0x04000006 RID: 6
			private UserResultDto _user;
		}

		// Token: 0x02000005 RID: 5
		[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
		private sealed class TestUserService_ClearCache<>n___ : SpringCachingRequirementProxy
		{
			// Token: 0x06000018 RID: 24 RVA: 0x000022E8 File Offset: 0x000004E8
			public TestUserService_ClearCache<>n___(TestUserService_BB93BE8338034C5FBC946FB266372676 _this_Service)
			{
				this._this_Service = _this_Service;
			}

			// Token: 0x17000005 RID: 5
			// (get) Token: 0x06000019 RID: 25 RVA: 0x00002308 File Offset: 0x00000508
			[Nullable(new byte[]
			{
				2,
				1,
				1
			})]
			public override IDictionary<string, object> Arguments
			{
				get
				{
					return new Dictionary<string, object>();
				}
			}

			// Token: 0x0600001A RID: 26 RVA: 0x0000231C File Offset: 0x0000051C
			private ICacheableRequirement GetCacheEvictRequirement_0()
			{
				return new CacheEvictRequirement("users")
				{
					AllEntries = true
				};
			}

			// Token: 0x0600001B RID: 27 RVA: 0x0000233C File Offset: 0x0000053C
			public override IList<ICacheEvictRequirement> GetCacheEvictRequirements()
			{
				return new ICacheEvictRequirement[]
				{
					this.GetCacheEvictRequirement_0()
				};
			}

			// Token: 0x0600001C RID: 28 RVA: 0x00002360 File Offset: 0x00000560
			internal Task ClearCache<>n__()
			{
				return this._this_Service.ClearCache<>n__();
			}

			// Token: 0x04000007 RID: 7
			private TestUserService_BB93BE8338034C5FBC946FB266372676 _this_Service;
		}
	}
```
</details>