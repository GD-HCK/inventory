using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Inventory.Authentication.Handlers
{
    /// <summary>
    /// Provides a default authentication handler for unsupported or invalid authentication schemes.
    /// Returns a 401 Unauthorized response with a descriptive error message.
    /// </summary>
    /// <param name="options">The options monitor for authentication scheme options.</param>
    /// <param name="logger">The logger factory for logging authentication events.</param>
    /// <param name="encoder">The URL encoder.</param>
    internal class DefaultAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
        ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        /// <summary>
        /// Handles the authentication challenge for unsupported authentication schemes.
        /// Returns a 401 Unauthorized response with a JSON error message indicating valid authentication options.
        /// </summary>
        /// <param name="properties">Authentication properties associated with the challenge.</param>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.ContentType = "application/json";

            var headers = Request.Headers.ToDictionary(
                    h => h.Key,
                    h => h.Value.ToString()
                );

            string message = JsonSerializer.Serialize(new
            {
                statuscode = Response.StatusCode,
                error = "Unauthorized",
                message = "Invalid authentication scheme. Valid options are ApiKey, Basic or OpenIdConnect",
                headers
            });

            await Response.WriteAsync(message);
            await Response.CompleteAsync();
        }

        /// <summary>
        /// Always fails authentication, indicating that the authentication scheme is invalid.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthenticateResult"/> representing a failed authentication attempt.
        /// </returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid authentication scheme"));
        }
    }
}
