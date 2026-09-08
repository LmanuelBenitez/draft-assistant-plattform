using backend.Data;
using backend.Repositories.Implementations;
using backend.Repositories.Interfaces;
using backend.Services.Config;
using backend.Services.Implementations;
using backend.Services.Interfaces;
using backend.Validators;
using FluentValidation;
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
                options.UseSqlServer(connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

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
        /// Configura Swagger/OpenAPI
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Draft Assistant Platform API",
                    Version = "v1",
                    Description = "API para gestión de predicciones deportivas",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Draft Assistant Team",
                        Email = "support@draftassistant.com"
                    }
                });

                // Configurar para usar comentarios XML
                var xmlFile = $"{System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
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
