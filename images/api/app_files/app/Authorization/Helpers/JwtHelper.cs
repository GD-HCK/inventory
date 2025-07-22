using DataLibrary.Classes;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Inventory.Authorization.Helpers
{
    /// <summary>
    /// Provides helper methods for issuing, validating, and extracting claims from JWT tokens for authentication and authorization.
    /// </summary>
    /// <remarks>
    /// The <see cref="JwtHelper"/> class includes utilities for generating JWT tokens, validating tokens and claims, and mapping claim types to standard JWT names.
    /// </remarks>
    internal static class JwtHelper
    {
        /// <summary>
        /// Maps claim types to standard JWT claim names.
        /// </summary>
        internal static readonly Dictionary<string, string> ClaimTypeMap = new()
        {
            // Map claim types to JWT standard names
            ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] = "sub",
            ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] = "username",
            ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] = "email",
            ["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] = "role",
            // "name", "jti", "exp", "iss", "aud", and role are left as-is or handled below
        };

        /// <summary>
        /// Flattens a collection of claims into a dictionary, mapping claim types to standard JWT names.
        /// </summary>
        /// <param name="claims">The collection of claims to flatten.</param>
        /// <returns>A dictionary of claim names and values.</returns>
        internal static Dictionary<string, object> FlattenClaims(IEnumerable<Claim> claims)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var claim in claims)
            {
                // Use mapped name if available, otherwise use the original type
                var key = ClaimTypeMap.TryGetValue(claim.Type, out var mapped) ? mapped : claim.Type;

                // If the claim is "exp" and is a string number, convert to int
                if (key == "exp" && long.TryParse(claim.Value, out var expVal))
                    dict[key] = expVal;
                else
                    dict[key] = claim.Value;
            }

            return dict;
        }

        /// <summary>
        /// Issues a JWT token for the specified account and roles using the provided configuration.
        /// </summary>
        /// <param name="configuration">The application configuration containing JWT settings.</param>
        /// <param name="account">The account for which to issue the token.</param>
        /// <param name="roles">The roles to include in the token.</param>
        /// <returns>A signed JWT token string.</returns>
        /// <exception cref="NullReferenceException">Thrown if required JWT configuration is missing.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the secret key is not a valid Base64 string.</exception>

        internal static string IssueJwtToken(IConfiguration configuration, Account account, IList<AccountRole> roles)
        {
            var secretKey = configuration["Authentication:Jwt:SecretKey"] ?? throw new NullReferenceException("Invalid JWT configuration");
            var issuer = configuration["Authentication:Jwt:Issuer"] ?? throw new NullReferenceException("Invalid JWT configuration");
            var audience = configuration["Authentication:Jwt:Audience"] ?? throw new NullReferenceException("Invalid JWT configuration");
            var TokenLifetimeMinutes = int.Parse(configuration["Authentication:Jwt:TokenLifetimeMinutes"] ?? throw new NullReferenceException("Invalid JWT configuration"));

            var tokenExpiry = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes);

            // Generate JWT
            // https://jwt.io/
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, account.Id),
                new (JwtRegisteredClaimNames.UniqueName, account.UserName!),
                new (JwtRegisteredClaimNames.Email, account.Email!),
                new (JwtRegisteredClaimNames.Name, account.Name!),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Name!)));

            Byte[] convertedSecret;
            try
            {
                convertedSecret = Convert.FromBase64String(secretKey);
            }
            catch
            {
                throw new InvalidOperationException("The JWT Secret key is not a valid Base64 string");
            }

            var key = new SymmetricSecurityKey(convertedSecret);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: tokenExpiry,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Gets the token validation parameters for validating JWT tokens.
        /// </summary>
        /// <param name="secret">The secret key for signing the token.</param>
        /// <param name="issuer">The expected issuer.</param>
        /// <param name="audience">The expected audience.</param>
        /// <returns>A <see cref="TokenValidationParameters"/> object configured for JWT validation.</returns>

        internal static TokenValidationParameters GetTokenValidationParameters(string secret, string issuer, string audience)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secret)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Optional: Set to zero to avoid clock skew issues
            };
        }

        /// <summary>
        /// Verifies that the required claims are present in the provided claims collection.
        /// </summary>
        /// <param name="claims">The collection of claims to verify.</param>
        /// <returns>An <see cref="IdentityResult"/> indicating success or failure.</returns>

        internal static IdentityResult VerifyClaims(IEnumerable<Claim> claims)
        {
            var requiredClaims = new[]
            {
                    JwtRegisteredClaimNames.Sub,
                    JwtRegisteredClaimNames.Jti,
                    ClaimTypes.Role
                };

            var principleClaimsTransformed = claims.Select(
                c => ClaimTypeMap.TryGetValue(c.Type, out var mapped) ? mapped : c.Type
            );

            var requiredClaimsTransformed = requiredClaims.Select(
                c => ClaimTypeMap.TryGetValue(c, out var mapped) ? mapped : c
            );

            foreach (var rc in requiredClaimsTransformed)
            {
                if (!principleClaimsTransformed.Any(c => c == rc))
                {
                    return IdentityResult.Failed(
                    [
                        new IdentityError { Code = "MissingClaim", Description = $"Token is missing required claim: {rc}" }
                    ]);
                }
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Validates a JWT token and returns the associated <see cref="ClaimsPrincipal"/> if valid.
        /// </summary>
        /// <param name="issuer">The expected issuer.</param>
        /// <param name="audience">The expected audience.</param>
        /// <param name="secret">The secret key for signing the token.</param>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>The <see cref="ClaimsPrincipal"/> extracted from the token.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the token is invalid or required claims are missing.</exception>
        internal static ClaimsPrincipal VerifyToken(string issuer, string audience, string secret, string token)
        {
            ArgumentNullException.ThrowIfNull(secret, nameof(secret));
            ArgumentNullException.ThrowIfNull(issuer, nameof(issuer));
            ArgumentNullException.ThrowIfNull(audience, nameof(audience));
            ArgumentNullException.ThrowIfNull(token, nameof(token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, GetTokenValidationParameters(secret, issuer, audience), out _);

            var claimVerificationResult = VerifyClaims(principal.Claims);

            if (claimVerificationResult.Succeeded)
            {
                return principal;
            }
            else
            {
                throw new UnauthorizedAccessException(claimVerificationResult.Errors.FirstOrDefault()?.Description ?? "Token verification failed");
            }
        }

        /// <summary>
        /// Extracts and flattens claims from a JWT token using the provided configuration and secret.
        /// </summary>
        /// <param name="configuration">The application configuration containing JWT settings.</param>
        /// <param name="secret">The secret key for signing the token.</param>
        /// <param name="token">The JWT token to extract claims from.</param>
        /// <returns>A dictionary of claim names and values.</returns>
        internal static Dictionary<string, object> GetTokenClaims(IConfiguration configuration, string secret, string token)
        {
            var issuer = configuration["Authentication:Jwt:Issuer"];
            var audience = configuration["Authentication:Jwt:Audience"];

            ArgumentNullException.ThrowIfNull(issuer, nameof(issuer));
            ArgumentNullException.ThrowIfNull(audience, nameof(audience));

            var principal = VerifyToken(issuer, audience, secret, token);

            return FlattenClaims(principal.Claims);
        }
    }
}
