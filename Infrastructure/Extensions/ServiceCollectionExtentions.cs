using Application.Common.Interfaces.Notifications;
using Application.Common.Interfaces.Repositories;
using Application.Interfaces.Auth;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.UnitOfWork;
using Application.Services;
using brevo_csharp.Api;
using Domain.Common.Security;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Seeding;
using Infrastructure.Persistence.UnitOfWork;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Services.AI;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Caching;
using Infrastructure.Services.Email;
using Infrastructure.Services.GoogleMaps;
using Infrastructure.Services.Notifications;
using Infrastructure.Services.Storage;
using Infrastructure.Services.Storage.Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IVerificationService, VerificationService>();
            services.AddScoped<IAuthService, JwtService>();
            services.AddScoped<IInAppNotificationService, InAppNotificationService>();
            services.AddScoped<INotificationService, NotificationService>();
            //services.AddScoped<IAgencyNotifier, AgencyNotifier>();
            //services.AddScoped<IResponderNotifier, ResponderNotifier>();


            return services;
        }

        // public static IServiceCollection AddDbConnection(this IServiceCollection services, IConfiguration configuration)
        // {
        //     var conn = "";

        //     // //Console.WriteLine(conn);
        //     // if (string.IsNullOrWhiteSpace(conn))
        //     // {
        //         var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        //         if (!string.IsNullOrWhiteSpace(databaseUrl))
        //         {
        //             var uri = new Uri(databaseUrl);
        //             var userInfo = uri.UserInfo.Split(':');

        //             conn =
        //                 $"Host={uri.Host};" +
        //                 $"Port=5432;" +
        //                 $"Database={uri.AbsolutePath.Trim('/')};" +
        //                 $"Username={userInfo[0]};" +
        //                 $"Password={userInfo[1]};" +
        //                 $"SSL Mode=Require;Trust Server Certificate=true";
        //         }
        //     // }

        //     services.AddDbContext<ProjectDbContext>(options =>
        //         options.UseNpgsql(conn)
        //         .EnableDetailedErrors()
        //     );

        //     services.AddScoped<IUnitOfWork, UnitOfWork>();

        //     return services;
        // }

        public static IServiceCollection AddDbConnection(this IServiceCollection services, IConfiguration configuration)
        {
            string? conn;

            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':', 2);

                conn =
                    $"Host={uri.Host};" +
                    $"Port={(uri.Port > 0 ? uri.Port : 5432)};" +
                    $"Database={uri.AbsolutePath.Trim('/')};" +
                    $"Username={userInfo[0]};" +
                    $"Password={userInfo[1]};" +
                    $"SSL Mode=Require;" +
                    $"Trust Server Certificate=true";
            }
            else
            {
                conn = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException(
                        "No database connection string configured.");
            }

            services.AddDbContext<ProjectDbContext>(options =>
                options.UseNpgsql(conn)
                       .EnableDetailedErrors());

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }


        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();

            return services;
        }

        public static IServiceCollection AddSecurity(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

            return services;
        }

        public static async Task SeedDatabaseAsync(this IServiceProvider services)
        {
            var context = services.GetRequiredService<ProjectDbContext>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();

            await DbInitializer.SeedAsync(context, passwordHasher);
        }

        public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(provider =>
            {
                var config = new brevo_csharp.Client.Configuration();
                var apiKey = configuration["Brevo:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                    throw new InvalidOperationException("Brevo API Key is not configured!");

                config.AddApiKey("api-key", apiKey);

                return new TransactionalEmailsApi(config);
            });

            services.AddScoped<IEmailService, BrevoEmailService>();

            return services;
        }

        public static IServiceCollection AddAIService(this IServiceCollection services)
        {
            services.AddHttpClient<OpenAIIncidentAnalyzer>();
            services.AddScoped<IAIService, OpenAIIncidentAnalyzer>();

            return services;
        }

        public static IServiceCollection AddGeocodingService(this IServiceCollection services)
        {
            services.AddHttpClient<GoogleMapsGeocodingService>();
            services.AddScoped<IGeocodingService, GoogleMapsGeocodingService>();

            return services;
        }

        public static IServiceCollection AddStorageService(this IServiceCollection services, string webRootPath)
        {
            services.AddSingleton(new LocalStorageService(webRootPath));
            services.AddSingleton<CloudinaryStorageService>();

            services.AddScoped<IStorageManager, StorageManager>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();
            services.AddScoped<IResponderRepository, ResponderRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IIncidentRepository, IncidentRepository>();
            services.AddScoped<IIncidentResponderRepository, IncidentResponderRepository>();
            //services.AddScoped<IIncidentLiveStreamRepository, IncidentLiveStreamRepository>();
            //services.AddScoped<IIncidentMediaRepository, IncidentMediaRepository>();
            //services.AddScoped<IIncidentLocationUpdateRepository, IncidentLocationUpdateRepository>();
            //services.AddScoped<IChatRepository, ChatRepository>();
            //services.AddScoped<IChatParticipantRepository, ChatParticipantRepository>();
            //services.AddScoped<IMessageRepository, MessageRepository>();
            //services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IDashboardTrendsService, DashboardTrendsService>();

            return services;
        }

    }
}
