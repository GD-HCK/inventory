using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Inventory.Swagger.OperationFilters
{
    /// <summary>
    /// Adds security requirements to Swagger operations based on authorization attributes.
    /// </summary>
    /// <remarks>
    /// The <see cref="AuthorizeCheckOperationFilter"/> class implements <see cref="IOperationFilter"/> to automatically add security requirements to Swagger documentation.
    /// It checks for the presence of <see cref="AllowAnonymousAttribute"/> and applies the appropriate security schemes (Bearer, ApiKey, Basic) to each operation.
    /// </remarks>
    internal class AuthorizeCheckOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies security requirements to the Swagger operation based on authorization attributes.
        /// </summary>
        /// <param name="operation">The Swagger operation to modify.</param>
        /// <param name="context">The context containing method and controller metadata.</param>
        /// <remarks>
        /// Adds a Bearer security requirement to all operations except those marked with <see cref="AllowAnonymousAttribute"/>.
        /// For the "Authorize" method, adds both ApiKey and Basic security requirements.
        /// </remarks>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!context.MethodInfo.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) &&
                !(context.MethodInfo.DeclaringType?.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) ?? false))
            {
                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme {
                                Reference = new OpenApiReference {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            }, Array.Empty<string>()
                        }
                    }
                ];
            }

            if (context.MethodInfo.Name == "Authorize")
            {
                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme {
                                Reference = new OpenApiReference {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "ApiKey"
                                }
                            }, Array.Empty<string>()
                        }
                    },
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme {
                                Reference = new OpenApiReference {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Basic"
                                }
                            }, Array.Empty<string>()
                        }
                    }
                ];
            }
        }
    }
}
