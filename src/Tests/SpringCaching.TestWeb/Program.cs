using SpringCaching.Tests;
using SpringCaching.TestWeb;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.AddJsonAppSetingsIfExists("appsettings.Local.json");

// Add services to the container.
builder.Services.AddMvc();

builder.Services.AddScoped<ITestService, TestService>();

string redisConnectionString = builder.Configuration.GetConnectionString("RedisConnectionString");

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "SpringCaching.TestWeb:";
    });
}

builder.Services.AddSpringCaching();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

//app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
