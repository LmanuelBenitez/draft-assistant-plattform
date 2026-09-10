using backend.Data;
using backend.Repositories.Implementations;
using backend.Repositories.Interfaces;
using backend.Services.Config;
using backend.Services.Implementations;
using backend.Services.Interfaces;
using backend.Validators;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace backend.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configura los servicios de base de datos
        /// </summary>
        public static IServiceCollection AddDatabaseServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            return services;
        }

        /// <summary>
        /// Configura los servicios de la aplicación
        /// </summary>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuración de DeepSeek
            services.Configure<DeepSeekConfig>(
                configuration.GetSection("DeepSeek"));

            // Registro de servicios
            services.AddScoped<IPrediccionService, PrediccionService>();
            services.AddScoped<IPoissonService, PoissonService>();
            services.AddScoped<IDeepSeekService, DeepSeekService>();

            // Registro de repositorios
            services.AddScoped<IPrediccionRepository, PrediccionRepository>();

            // Registro de validadores
            services.AddScoped<IValidator<DTOs.Request.PartidoRequestDto>, PartidoRequestValidator>();

            // HttpClient para DeepSeek
            services.AddHttpClient<IDeepSeekService, DeepSeekService>(client =>
            {
                var deepseekConfig = configuration.GetSection("DeepSeek").Get<DeepSeekConfig>();
                client.BaseAddress = new Uri(deepseekConfig.BaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {deepseekConfig.ApiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            return services;
        }

        /// <summary>
        /// Configura Api de Football
        /// </summary>
        public static IServiceCollection AddFootballApiService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuración de FootballApi
            services.Configure<FootballApiConfig>(
                configuration.GetSection("FootballApi"));
            // Registrar FootballApiService
            services.AddScoped<IFootballApiService, FootballApiService>();
            // Registro de servicio
            services.AddHttpClient<IFootballApiService, FootballApiService>(client =>
            {
                var footballApiConfig = configuration.GetSection("FootballApi").Get<FootballApiConfig>();
                client.BaseAddress = new Uri(footballApiConfig.BaseUrl);
                client.DefaultRequestHeaders.Add("x-apisports-key", footballApiConfig.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            return services;
        }

        /// <summary>
        /// Configura CORS
        /// </summary>
        public static IServiceCollection AddCorsPolicies(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:4200" };

            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", builder =>
                {
                    if (allowedOrigins.Length == 1 && allowedOrigins[0] == "*")
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    }
                    else
                    {
                        builder.WithOrigins(allowedOrigins)
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    }
                });
            });

            return services;
        }

        /// <summary>
        /// Configura los validadores de FluentValidation
        /// </summary>
        public static IServiceCollection AddFluentValidationServices(
            this IServiceCollection services)
        {
            // Registrar todos los validadores en el assembly
            services.AddValidatorsFromAssemblyContaining<PartidoRequestValidator>();

            return services;
        }
    }
}
