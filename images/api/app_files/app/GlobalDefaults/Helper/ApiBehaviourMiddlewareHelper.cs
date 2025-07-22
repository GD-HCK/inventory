using Microsoft.AspNetCore.Mvc;

namespace Inventory.GlobalDefaults.Helper
{
    /// <summary>
    /// Provides helper methods for configuring API behavior middleware, 
    /// specifically for handling invalid model state responses.
    /// </summary>
    internal static class ApiBehaviourMiddlewareHelper
    {
        /// <summary>
        /// Configures the application's behavior for invalid model state responses.
        /// Sets a custom response factory that returns a detailed JSON error response
        /// when model validation fails.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to which the API behavior options will be added.
        /// </param>
        internal static void ConfigureApiInvalidModelStateBehavior(IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    // Check for empty body or deserialization errors
                    var hasBodyError = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Any(e => e.ErrorMessage.Contains("body is required") || e.ErrorMessage.Contains("field is required"));

                    var result = new JsonResult(new
                    {
                        status = StatusCodes.Status400BadRequest,
                        message = hasBodyError
                            ? "Invalid or missing request body. Ensure you are sending a valid JSON object matching the Server schema."
                            : "One or more validation errors occurred.",
                        errors = context.ModelState
                            .Where(x => x.Value!.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    })
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        ContentType = "application/json"
                    };

                    return result;
                };
            });
        }
    }
}
