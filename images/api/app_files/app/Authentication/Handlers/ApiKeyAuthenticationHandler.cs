using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Inventory.Authentication.Handlers
{
    /// <summary>
    /// Handles API key authentication for incoming HTTP requests.
    /// Validates the presence and correctness of an API key in the request headers and authenticates the user accordingly.
    /// </summary>
    /// <remarks>
    /// This handler expects an <c>ApiKey</c> header in the request. If the API key is valid and associated with an account,
    /// the request is authenticated; otherwise, an unauthorized response is returned.
    /// </remarks>
    /// <param name="credentialContextHelper">Helper for retrieving and validating account credentials.</param>
    /// <param name="options">The options monitor for authentication scheme options.</param>
    /// <param name="logger">The logger factory for logging authentication events.</param>
    /// <param name="encoder">The URL encoder.</param>
    internal class ApiKeyAuthenticationHandler(
        IAccountContextHelper credentialContextHelper,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
        ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        private const string scheme = "ApiKey";
        private const string HeaderKeyName = "ApiKey";
        private readonly IAccountContextHelper _credentialContextHelper = credentialContextHelper;
        private string? contextError;

        /// <summary>
        /// Handles the challenge when authentication fails.
        /// Returns a 401 Unauthorized response with a JSON error message describing the failure.
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

            string message;

            if (!string.IsNullOrEmpty(contextError))
            {
                message = JsonSerializer.Serialize(new
                {
                    scheme,
                    statuscode = Response.StatusCode,
                    error = "Unauthorized",
                    message = contextError,
                    headers
                });
            }
            else if (!Request.Headers.ContainsKey(HeaderKeyName))
            {
                message = JsonSerializer.Serialize(new
                {
                    scheme,
                    statuscode = Response.StatusCode,
                    error = "Unauthorized",
                    message = "Include an ApiKey header in your request",
                    headers
                });
            }
            else
            {
                message = JsonSerializer.Serialize(new
                {
                    scheme,
                    statuscode = Response.StatusCode,
                    error = "Unauthorized",
                    message = "Please provide a valid Apikey.",
                    headers
                });
            }

            await Response.WriteAsync(message);
            await Response.CompleteAsync();
        }

        /// <summary>
        /// Attempts to authenticate the request using the API key provided in the request headers.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthenticateResult"/> indicating success if the API key is valid, or failure otherwise.
        /// </returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var remoteIpAddress = Context.Connection.RemoteIpAddress?.MapToIPv4();
                if (remoteIpAddress == null)
                {
                    contextError = "Remote IP address is not available";
                    return AuthenticateResult.Fail(contextError);
                }

                if (!Request.Headers.TryGetValue(HeaderKeyName, out var apiKeyHeaderValues))
                    return AuthenticateResult.Fail("API Key was not provided.");

                var credential = new Account()
                {
                    ApiKey = apiKeyHeaderValues.FirstOrDefault()!
                };

                var apikey = await _credentialContextHelper.GetAccountAsync(credential, remoteIpAddress);
                var claims = new[] { new Claim(ClaimTypes.Name, scheme) };
                var identity = new ClaimsIdentity(claims, scheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, scheme);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception e)
            {
                Logger.LogDebug("An exception occurred during authentication: {Message}", e.Message);
                contextError = e.Message;
                return AuthenticateResult.Fail(e.Message);
            }
        }
    }
}
