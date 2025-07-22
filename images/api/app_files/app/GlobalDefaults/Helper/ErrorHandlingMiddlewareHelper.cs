using Microsoft.AspNetCore.Diagnostics;

namespace Inventory.ErrorHandling.Helper
{
    /// <summary>
    /// Provides helper methods for configuring global error handling middleware in the application.
    /// </summary>
    internal static class ErrorHandlingMiddlewareHelper
    {
        /// <summary>
        /// Configures a global exception handler for the application.
        /// Maps known exception types to appropriate HTTP status codes and returns a JSON error response.
        /// </summary>
        /// <param name="app">
        /// The <see cref="WebApplication"/> to which the error handling middleware will be added.
        /// </param>
        internal static void ConfigureDefaultErrorHandler(WebApplication app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;
                    
                    logger.LogError(exception, "Unhandled exception");

                    // Default to 500
                    var statusCode = StatusCodes.Status500InternalServerError;

                    // Map known exception types to status codes
                    if (exception is UnauthorizedAccessException)
                        statusCode = StatusCodes.Status401Unauthorized;
                    else if (exception is ArgumentException)
                        statusCode = StatusCodes.Status400BadRequest;
                    else if (exception is KeyNotFoundException)
                        statusCode = StatusCodes.Status404NotFound;
                    // Add more mappings as needed, including custom exceptions

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";

                    var error = new
                    {
                        Error = exceptionHandlerPathFeature?.Error.Message,
                        InnerException = exceptionHandlerPathFeature?.Error.InnerException?.Message
                    };

                    await context.Response.WriteAsJsonAsync(error);
                });
            });
        }
    }
}
