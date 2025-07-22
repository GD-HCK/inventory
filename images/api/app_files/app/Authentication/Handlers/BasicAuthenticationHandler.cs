using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Inventory.Authentication.Handlers
{
    /// <summary>
    /// Handles HTTP Basic authentication for incoming requests.
    /// Validates the presence and correctness of the Authorization header and authenticates the user using provided credentials.
    /// </summary>
    /// <remarks>
    /// This handler expects an <c>Authorization</c> header with the value "Basic &lt;base64-encoded-credentials&gt;".
    /// The credentials must be a base64-encoded string in the format "username:password".
    /// </remarks>
    /// <param name="credentialContextHelper">Helper for retrieving and validating account credentials.</param>
    /// <param name="options">The options monitor for authentication scheme options.</param>
    /// <param name="logger">The logger factory for logging authentication events.</param>
    /// <param name="encoder">The URL encoder.</param>
    internal class BasicAuthenticationHandler(
        IAccountContextHelper credentialContextHelper,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
        ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        /// <summary>
        /// The authentication scheme name for Basic authentication.
        /// </summary>
        public const string scheme = "Basic";
        /// <summary>
        /// The name of the HTTP header used for Basic authentication.
        /// </summary>
        public const string HeaderKeyName = "Authorization";

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
                    message = "Include an Authorization header in your request",
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
                    message = "Please provide a valid Base64 credentials string.",
                    headers
                });
            }

            await Response.WriteAsync(message);
            await Response.CompleteAsync();
        }

        /// <summary>
        /// Attempts to authenticate the request using the Basic credentials provided in the Authorization header.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthenticateResult"/> indicating success if the credentials are valid, or failure otherwise.
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

                if (!Request.Headers.TryGetValue("Authorization", out StringValues value))
                    throw new InvalidOperationException("Missing Authorization Header");

                var authHeader = value.ToString();
                if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Invalid Authorization Header");

                var token = authHeader["Basic ".Length..].Trim();
                var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var credentials = credentialString.Split(':');
                if (credentials.Length != 2)
                    throw new InvalidOperationException("Invalid Basic Authentication Credentials. The string does not resolve to username:password format");

                var username = credentials[0];
                var password = credentials[1];

                var credential = new Account()
                {
                    UserName = username,
                    Password = password
                };

                var creds = await _credentialContextHelper.GetAccountAsync(credential, remoteIpAddress);

                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, scheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, scheme);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception e)
            {
                Logger.LogDebug("An error occurred during authentication: {ErrorMessage}", e.Message);
                contextError = e.Message;
                return AuthenticateResult.Fail(e.Message);
            }
        }
    }
}
