using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Context
{
    public class ProjectDbContextFactory : IDesignTimeDbContextFactory<ProjectDbContext>
    {
        public ProjectDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Host");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("AppString");


            //Console.WriteLine($"✅ Using Connection String: {connectionString}");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                if (!string.IsNullOrWhiteSpace(databaseUrl))
                {
                    var uri = new Uri(databaseUrl);
                    var userInfo = uri.UserInfo.Split(':');

                    connectionString =
                        $"Host={uri.Host};" +
                        $"Port={uri.Port};" +
                        $"Database={uri.AbsolutePath.Trim('/')};" +
                        $"Username={userInfo[0]};" +
                        $"Password={userInfo[1]};" +
                        $"SSL Mode=Require;Trust Server Certificate=true";
                }
            }

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'AppString' is missing or empty in appsettings.json.");
            
            var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ProjectDbContext(optionsBuilder.Options);
        }
    }
}