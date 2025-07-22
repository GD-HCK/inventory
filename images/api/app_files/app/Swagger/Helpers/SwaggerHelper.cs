using Inventory.Swagger.OperationFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Runtime.Intrinsics;

namespace Inventory.Swagger.Helpers
{
    /// <summary>
    /// Provides helper methods for configuring Swagger/OpenAPI documentation and security for the API.
    /// </summary>
    /// <remarks>
    /// The <see cref="SwaggerHelper"/> class centralizes the setup of Swagger generation, including API versioning, security definitions, and XML comments integration.
    /// </remarks>
    internal static class SwaggerHelper
    {
        /// <summary>
        /// Configures Swagger/OpenAPI generation and security schemes for the API.
        /// </summary>
        /// <param name="services">The service collection to add Swagger services to.</param>
        /// <param name="appVersion">The version string to use for the Swagger document.</param>
        /// <remarks>
        /// Registers the Swagger generator, sets up API versioning, and adds security definitions for Bearer, ApiKey, and Basic authentication.
        /// Also includes XML comments for enhanced documentation and applies the <see cref="AuthorizeCheckOperationFilter"/> to secure endpoints.
        /// </remarks>
        internal static void ConfigureSwagger(IServiceCollection services, string appVersion)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(appVersion, new OpenApiInfo
                {
                    Version = appVersion,
                    Title = "Server inventory API",
                    Description = "An ASP.NET Core Web API for managing the servers inventory"

                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token. This is for all endpoints but Authorize",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid ApiKey. Use this to issue a JWT token via the Authorize endpoint",
                    Name = "ApiKey",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ApiKey"
                });

                options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid username and password. Use this to issue a JWT token via the Authorize endpoint",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Basic"
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

        }
    }
}
