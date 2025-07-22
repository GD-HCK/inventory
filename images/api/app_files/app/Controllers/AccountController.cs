using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Inventory.Authorization.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Inventory.Controllers
{
    /// <summary>
    /// Provides endpoints for account management, including account creation, authentication, and JWT token verification.
    /// </summary>
    /// <remarks>
    /// The <see cref="AccountController"/> class exposes API endpoints for generating new accounts, issuing JWT tokens for authentication, and verifying JWT tokens.
    /// It leverages <see cref="IAccountContextHelper"/> for account operations and <see cref="JwtHelper"/> for token handling.
    /// </remarks>
    [ApiController] // marks the controller behaviour for Api
    [Route("[controller]")] // sets the api  route to match the name of the class without Controller so /weatherforecast
    [AllowAnonymous]
    public class AccountController(IAccountContextHelper accountContextHelper, IConfiguration configuration) : ControllerBase
    {
        private readonly IAccountContextHelper _accountContextHelper = accountContextHelper;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Creates a new account with optional IP restrictions and role assignment.
        /// </summary>
        /// <param name="roleType">The role to assign to the new account (e.g., Admin, User, Guest).</param>
        /// <param name="restrictIp">If true, restricts the account to the caller's IP address.</param>
        /// <param name="restrictRange">If true, restricts the account to the caller's IP range (/24).</param>
        /// <returns>The newly created <see cref="Account"/> object.</returns>
        /// <remarks>
        /// Sample requests:
        ///
        ///     GET /account
        ///     GET /account?restrictIp=true
        ///     GET /account?restrictRange=true
        ///
        /// </remarks>
        /// <response code="201">Account created successfully.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Account>> AddAccount([FromQuery] RoleType roleType, [FromQuery] bool restrictIp, [FromQuery] bool restrictRange)
        {
            var creds = await _accountContextHelper.AddAccountAsync(HttpContext.Connection.RemoteIpAddress?.MapToIPv4(), restrictIp, restrictRange, roleType);
            return Created(string.Empty, creds);
        }

        /// <summary>
        /// Authenticates an account using an API key or Basic credentials and issues a JWT token.
        /// </summary>
        /// <returns>A JSON object containing the JWT token if authentication is successful.</returns>
        /// <remarks>
        /// Sample requests:
        ///
        ///     POST /account/token
        ///     Headers:
        ///         Authorization: Basic {base64(username:password)}
        ///     OR
        ///         ApiKey: {apiKey}
        ///
        /// </remarks>
        /// <response code="200">Account authorized and token issued.</response>
        /// <response code="401">Invalid credentials or unauthorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Authorize()
        {
            string? apiKey = Request.Headers["ApiKey"];
            string? authHeader = Request.Headers.Authorization;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4() ?? throw new NullReferenceException("Remote IP address is not available");

            Account tempAccount;

            if (HttpContext?.User?.Identity?.IsAuthenticated ?? false)
            {
                return Ok("User already authenticated");
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                tempAccount = new Account { ApiKey = apiKey };
            }
            else if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic "))
            {
                var encoded = authHeader["Basic ".Length..].Trim();
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var parts = decoded.Split(':', 2);
                if (parts.Length == 2)
                {
                    tempAccount = new Account
                    {
                        UserName = parts[0],
                        Password = parts[1]
                    };
                }
                else
                {
                    return Unauthorized("Invalid credentials supplied");
                }
            }
            else
            {
                return Unauthorized("No credentials supplied");
            }

            Account account;

            try
            {
                account = await _accountContextHelper.GetAccountAsync(tempAccount, remoteIpAddress);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }


            IList<AccountRole> roles;

            try
            {
                roles = await _accountContextHelper.GetAccountRoles(account);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }

            var jwt = JwtHelper.IssueJwtToken(_configuration, account, roles);

            return Ok(new { token = jwt });
        }

        /// <summary>
        /// Verifies the validity of a JWT token and its signing key.
        /// </summary>
        /// <param name="token">The JWT token to verify (provided in the request header).</param>
        /// <param name="secret">The signing secret used to validate the token (provided in the request header).</param>
        /// <returns>A JSON object indicating whether the token is valid and, if so, its claims.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /account/token/verify
        ///     Headers:
        ///         token: {token}
        ///         secret: {secret}
        ///
        /// </remarks>
        /// <response code="200">Token and signing key are valid.</response>
        /// <response code="400">Invalid token or signing key.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("token/verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult VerifyToken([FromHeader] string token, [FromHeader] string secret)
        {
            ArgumentNullException.ThrowIfNull(token);
            ArgumentNullException.ThrowIfNull(secret);

            try
            {
                return Ok(new
                {
                    valid = true,
                    claims = JwtHelper.GetTokenClaims(_configuration, secret, token)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { valid = false, error = ex.Message });
            }
        }
    }
}
