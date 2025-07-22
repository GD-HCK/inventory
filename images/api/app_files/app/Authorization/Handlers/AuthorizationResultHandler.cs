using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Inventory.Authorization.Handlers
{
    /// <summary>
    /// Handles the result of authorization middleware, providing custom responses for failed authorization.
    /// </summary>
    /// <remarks>
    /// The <see cref="AuthorizationResultHandler"/> class implements <see cref="IAuthorizationMiddlewareResultHandler"/> to customize the HTTP response when authorization fails.
    /// If authorization is unsuccessful, it returns a 403 Forbidden status code with a JSON error message. Otherwise, it delegates to the default handler.
    /// </remarks>
    internal class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        /// <summary>
        /// Handles the authorization result for the current HTTP request.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="policy">The authorization policy to evaluate.</param>
        /// <param name="authorizeResult">The result of the authorization evaluation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// If authorization fails, sets the response status to 403 Forbidden and returns a JSON error message.
        /// If authorization succeeds, delegates to the default handler.
        /// </remarks>
        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (!authorizeResult.Succeeded)
            {
                // Try to get the failure reason from the context
                var failure = authorizeResult.AuthorizationFailure?.FailureReasons?.FirstOrDefault();
                var message = failure?.Message ?? "Unauthorized request";

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\": \"{message}\"}}");
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
