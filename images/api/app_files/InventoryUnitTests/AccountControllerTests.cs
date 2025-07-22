using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Inventory.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InventoryUnitTests
{
    public class AccountControllerTests
    {

        private static string KeyGen()
        {
            // Generate a 256-bit (32-byte) secret
            byte[] secret = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secret);
            }

            // Convert to a Base64 string
            return Convert.ToBase64String(secret);
        }

        private static AccountRole GenerateRole(string roleName)
        {
            AccountRole role = roleName.ToLower() switch
            {
                "guest" => new AccountRole()
                {
                    Name = "User",
                    Description = "Standard User with read permissions",
                    EndpointPermissions = []
                },
                "user" => new AccountRole()
                {
                    Name = "User",
                    Description = "Standard User with read permissions",
                    EndpointPermissions = [new EndpointPermission("server", [new EndpointPermissionAction(EndpointPermissionActionType.Read)])]
                },
                "admin" => new AccountRole()
                {
                    Name = "admin",
                    Description = "Administrator role with full access",
                    EndpointPermissions = [
                        new EndpointPermission("server", [new EndpointPermissionAction(EndpointPermissionActionType.Read)]),
                                new EndpointPermission("server", [new EndpointPermissionAction(EndpointPermissionActionType.Create)]),
                                new EndpointPermission("server", [new EndpointPermissionAction(EndpointPermissionActionType.Delete)]),
                                new EndpointPermission("server", [new EndpointPermissionAction(EndpointPermissionActionType.Update)]),
                            ]
                },
                _ => throw new ArgumentException("Invalid role name"),
            };

            return role;
        }

        // GET

        [Theory]
        [InlineData("10.0.0.1", false, false, "user")]
        [InlineData("10.0.0.1", true, false, "user")]
        [InlineData("10.0.0.1", false, true, "user")]
        [InlineData("10.0.0.1", true, true, "user")]

        [InlineData("10.0.0.1", false, false, "admin")]
        [InlineData("10.0.0.1", true, false, "admin")]
        [InlineData("10.0.0.1", false, true, "admin")]
        [InlineData("10.0.0.1", true, true, "admin")]
        public async Task AddAccountAsync_WithParams_ReturnsExpectedResult(string ipAddress, bool restrictIp, bool restrictRange, string roleName)
        {
            _ = IPAddress.TryParse(ipAddress, out IPAddress? ip);

            var mockAccountContextHelper = new Mock<IAccountContextHelper>();
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new AccountController(mockAccountContextHelper.Object, mockConfiguration.Object)
            {
                // Simulate header
                ControllerContext = new()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = ip;

            Enum.TryParse<RoleType>(roleName, true, out var enumRole);

            var account = new Account();
            account.SetProperties(ip, restrictIp, restrictRange, [roleName]);

            mockAccountContextHelper.Setup(x => x.AddAccountAsync(ip, restrictIp, restrictRange, enumRole)).ReturnsAsync(account);

            var result = await controller.AddAccount(enumRole, restrictIp, restrictRange);

            mockAccountContextHelper.Verify(x => x.AddAccountAsync(ip, restrictIp, restrictRange, enumRole), Times.Once);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var accountResult = Assert.IsType<Account>(createdResult.Value);
        }

        // POST

        [Theory]
        // allowedIp = "10.0.0.1" (matches remoteIp)
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.1", false, false, typeof(OkObjectResult), "user")]
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.1", false, true, typeof(OkObjectResult), "user")]
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.1", true, false, typeof(OkObjectResult), "user")]
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.1", true, true, typeof(OkObjectResult), "user")]

        [InlineData("ApiKey", "10.0.0.1", "10.0.0.1", false, false, typeof(OkObjectResult), "guest")]

        // allowedIp = "10.0.0.2" (does not match remoteIp)
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.2", false, false, typeof(UnauthorizedObjectResult), null)]
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.2", false, true, typeof(UnauthorizedObjectResult), null)]
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.2", true, false, typeof(UnauthorizedObjectResult), null)]
        [InlineData("ApiKey", "10.0.0.1", "10.0.0.2", true, true, typeof(UnauthorizedObjectResult), null)]

        // allowedIp = null (no restriction)
        [InlineData("ApiKey", "10.0.0.1", null, false, false, typeof(OkObjectResult), "admin")]
        [InlineData("ApiKey", "10.0.0.1", null, false, true, typeof(OkObjectResult), "admin")]
        [InlineData("ApiKey", "10.0.0.1", null, true, false, typeof(OkObjectResult), "admin")]
        [InlineData("ApiKey", "10.0.0.1", null, true, true, typeof(OkObjectResult), "admin")]
        public async Task Authorize_WithDifferentParams_ReturnsExpectedResult(string method, string remoteIp, string? allowedIp, bool restrictIp, bool restrictRange, Type type, string? roleName)
        {
            // create moc objects for dependencies
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:SecretKey"]).Returns(KeyGen());
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:Issuer"]).Returns("issuer");
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:Audience"]).Returns("audience");
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:TokenLifetimeMinutes"]).Returns("60");

            var mockAccountContextHelper = new Mock<IAccountContextHelper>();

            string ApiKey = KeyGen();
            string Base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("username:password"));

            _ = IPAddress.TryParse(remoteIp, out var _remoteIp);
            _ = IPAddress.TryParse(allowedIp, out IPAddress? _allowedIp);

            var account = new Account();

            account.SetProperties(_allowedIp, restrictIp, restrictRange, ["admin"]);

            if (!account.ValidateIpAddress(_remoteIp!))
            {
                mockAccountContextHelper
                    .Setup(x => x.GetAccountAsync(It.IsAny<Account>(), _remoteIp!))
                    .ThrowsAsync(new InvalidOperationException($"Unauthorized request for IP Address {_remoteIp!.MapToIPv4()}"));
            }
            else
            {
                // Setup the mock to return the account when GetAccountAsync is called with any Account object
                if (type == typeof(OkObjectResult))
                {
                    ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

                    mockAccountContextHelper.Setup(x => x.GetAccountAsync(It.IsAny<Account>(), _remoteIp!))
                        .ReturnsAsync(account);

                    AccountRole role = GenerateRole(roleName.ToLower());

                    mockAccountContextHelper.Setup(x => x.GetAccountRoles(account))
                        .ReturnsAsync([role]);

                    ApiKey = account.ApiKey!;
                    Base64Encoded = account.Base64Encoded!;
                }
                else
                {
                    mockAccountContextHelper
                        .Setup(x => x.GetAccountAsync(It.IsAny<Account>(), _remoteIp!))
                        .ThrowsAsync(new InvalidOperationException("Invalid credentials or account not found"));
                }
            }

            var controller = new AccountController(mockAccountContextHelper.Object, mockConfiguration.Object)
            {
                // Simulate header
                ControllerContext = new()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = _remoteIp;

            switch (method.ToLower())
            {
                case "apikey":
                    controller.ControllerContext.HttpContext.Request.Headers["ApiKey"] = ApiKey;
                    break;
                case "basicauth":
                    controller.ControllerContext.HttpContext.Request.Headers.Authorization = $"Basic {Base64Encoded}";
                    break;
                default:
                    throw new ArgumentException("Invalid authentication method");
            }

            IActionResult result = await controller.Authorize();

            if (type == typeof(OkObjectResult))
            {
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Contains("token", okResult.Value!.ToString());
            }
            else if (type == typeof(UnauthorizedObjectResult))
            {
                Assert.IsType<UnauthorizedObjectResult>(result);
            }
            else
            {
                throw new InvalidOperationException("Unexpected result type: " + type.Name);
            }
        }

        public static string IssueJwtToken(byte[] secret, List<Claim> claims, string audience, string issuer, int tokenExpiry)
        {
            var key = new SymmetricSecurityKey(secret);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expiry = DateTime.UtcNow.AddMinutes(tokenExpiry);
            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [Theory]
        [InlineData("ValidToken", typeof(OkObjectResult))]
        [InlineData("BadSecret", typeof(BadRequestObjectResult))]
        [InlineData("BadToken", typeof(BadRequestObjectResult))]
        [InlineData("EmptyToken", typeof(BadRequestObjectResult))]
        [InlineData("EmptySecret", typeof(BadRequestObjectResult))]
        [InlineData("ExpiredToken", typeof(BadRequestObjectResult))]
        [InlineData("BadIssuer", typeof(BadRequestObjectResult))]
        [InlineData("BadAudience", typeof(BadRequestObjectResult))]
        [InlineData("MissingClaims", typeof(BadRequestObjectResult))]
        public void VerifyToken_WithDifferentParams_ReturnsExpectedResult(string scenario, Type type)
        {
            var secret = KeyGen();
            var audience = "audience";
            var tokenExpiry = 60;
            var issuer = "issuer";

            // create moc objects for dependencies
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:SecretKey"]).Returns(secret);
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:Issuer"]).Returns(issuer);
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:Audience"]).Returns(audience);
            mockConfiguration.SetupGet(x => x["Authentication:Jwt:TokenLifetimeMinutes"]).Returns(tokenExpiry.ToString());

            var mockAccountContextHelper = new Mock<IAccountContextHelper>();
            var controller = new AccountController(mockAccountContextHelper.Object, mockConfiguration.Object)
            {
                // Simulate header
                ControllerContext = new()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new (JwtRegisteredClaimNames.UniqueName, "test"),
                new (JwtRegisteredClaimNames.Email, "test"),
                new (JwtRegisteredClaimNames.Name, "test"),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (ClaimTypes.Role, "admin")
            };

            Byte[] convertedSecret = [];
            string jwt = string.Empty;

            switch (scenario)
            {
                case "ValidToken":
                    convertedSecret = Convert.FromBase64String(secret);
                    jwt = IssueJwtToken(convertedSecret, claims, audience, issuer, tokenExpiry);
                    break;
                case "BadSecret":
                    convertedSecret = Convert.FromBase64String(KeyGen());
                    jwt = IssueJwtToken(convertedSecret, claims, audience, issuer, tokenExpiry);
                    break;
                case "BadToken":
                    jwt = "a8sdna89.ad3adasda.asdasdasd"; // Invalid token
                    break;
                case "EmptyToken":
                    jwt = string.Empty; // Empty token
                    break;
                case "EmptySecret":
                    convertedSecret = []; // Empty secret
                    jwt = "a8sdna89.ad3adasda.asdasdasd"; // Invalid token
                    break;
                case "ExpiredToken":
                    convertedSecret = Convert.FromBase64String(secret);
                    jwt = IssueJwtToken(convertedSecret, claims, audience, issuer, -60);
                    break;
                case "BadAudience":
                    convertedSecret = Convert.FromBase64String(secret);
                    jwt = IssueJwtToken(convertedSecret, claims, "AnotherAudience", issuer, tokenExpiry);
                    break;
                case "BadIssuer":
                    convertedSecret = Convert.FromBase64String(secret);
                    jwt = IssueJwtToken(convertedSecret, claims, audience, "AnotherIssuer", tokenExpiry);
                    break;
                case "MissingClaims":
                    convertedSecret = Convert.FromBase64String(secret);
                    jwt = IssueJwtToken(convertedSecret, [], audience, issuer, tokenExpiry);
                    break;
            }

            var result = controller.VerifyToken(jwt, secret);
            Assert.IsType(type, result);

        }
    }
}
