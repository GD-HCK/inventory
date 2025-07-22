using Inventory.Authorization.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Inventory.Authentication.Helper
{
    /// <summary>
    /// Provides helper methods for registering authentication handlers in the application.
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Registers and configures the JWT Bearer authentication handler for the application.
        /// </summary>
        /// <param name="services">The service collection to which the authentication handler will be added.</param>
        /// <param name="configuration">The application configuration containing JWT settings.</param>
        /// <exception cref="NullReferenceException">
        /// Thrown if required JWT configuration values (SecretKey, Issuer, Audience) are missing.
        /// </exception>
        public static void RegisterAuthenticationHandler(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = JwtHelper.GetTokenValidationParameters(
                configuration["Authentication:Jwt:SecretKey"] ?? throw new NullReferenceException("Invalid JWT Configuration - missing SecretKey"),
                configuration["Authentication:Jwt:Issuer"] ?? throw new NullReferenceException("Invalid JWT Configuration - missing Issuer"),
                configuration["Authentication:Jwt:Audience"] ?? throw new NullReferenceException("Invalid JWT Configuration - missing Audience")
            );

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var result = JwtHelper.VerifyClaims(context.Principal!.Claims);

                        if (!result.Succeeded)
                        {
                            context.Fail(result.Errors.FirstOrDefault()?.Description ?? "Token validation failed");
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
