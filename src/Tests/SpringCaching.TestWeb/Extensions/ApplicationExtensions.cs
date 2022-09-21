namespace SpringCaching.TestWeb
{
    public static class ApplicationExtensions
    {
        public static IWebHostBuilder AddJsonAppSetingsIfExists(this IWebHostBuilder webHostBuilder, string fileName)
        {
            if (File.Exists(Path.Combine(AppContext.BaseDirectory, fileName)))
            {
                webHostBuilder.ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    builder.AddJsonFile(fileName);
                });
            }
            return webHostBuilder;
        }
    }
}
