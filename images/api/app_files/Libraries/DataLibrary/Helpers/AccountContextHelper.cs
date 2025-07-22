using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace DataLibrary.Helpers
{
    public class AccountContextHelper(UserManager<Account> userManager, RoleManager<AccountRole> roleManager) : IAccountContextHelper
    {
        private readonly UserManager<Account> _userManager = userManager;
        private readonly RoleManager<AccountRole> _roleManager = roleManager;

        public Task<AccountRole> GetRoleByNameAsync(string roleName)
        {
            ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

            var query = from r in _roleManager.Roles
                        where r.Name == roleName
                        select r;

            var role = query
                .Include(r => r.EndpointPermissions!)
                .ThenInclude(ep => ep.EndpointPermissionActions!)
                .AsNoTracking()
                .FirstOrDefault() ?? throw new InvalidOperationException($"Role '{roleName}' not found.");

            return Task.FromResult(role);
        }

        private async Task<IdentityResult> CreateOrUpdateRoleAsync(string roleName)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    AccountRole newRole = GenerateAccountRole(roleName);
                    // Create new role
                    await _roleManager.CreateAsync(newRole);
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        private static AccountRole GenerateAccountRole (string roleName)
        {
            return roleName.ToLower() switch
            {
                "admin" => new AccountRole(
                    roleName.ToLower(),
                    "Administrator",
                    [
                        new EndpointPermission(
                                "All",
                                [
                                    new EndpointPermissionAction(EndpointPermissionActionType.Write)
                                ]
                            )
                    ]
                ),
                "priviledged" => new AccountRole(
                    roleName.ToLower(),
                    "Priviledged User",
                    [
                        new EndpointPermission(
                                "Server",
                                [
                                    new EndpointPermissionAction(EndpointPermissionActionType.Create),
                                    new EndpointPermissionAction(EndpointPermissionActionType.Read),
                                    new EndpointPermissionAction(EndpointPermissionActionType.Update)
                                ]
                            )
                    ]
                ),
                "singleendpoint" => new AccountRole(
                    roleName.ToLower(),
                    "User only one endpoint",
                    [
                        new EndpointPermission(
                                "Server/GetByIdAsync/Id",
                                [
                                    new EndpointPermissionAction(EndpointPermissionActionType.Read)
                                ]
                            )
                    ]
                ),
                "user" => new AccountRole(
                    roleName.ToLower(),
                    "Standard User",
                    [
                        new EndpointPermission(
                                "Server",
                                [
                                    new EndpointPermissionAction(EndpointPermissionActionType.Read)
                                ]
                            )
                    ]
                ),
                "guest" => new AccountRole(
                    roleName.ToLower(),
                    "Guest User",
                    []
                ),
                _ => throw new InvalidOperationException($"Unknown role name: {roleName}"),
            };
        }

        public async Task<Account> AddAccountAsync(IPAddress? ipAddress, bool restrictIp, bool restrictRange, RoleType roleType)
        {
            var roleName = roleType.ToString().ToLowerInvariant();

            var account = new Account();

            account.SetProperties(ipAddress, restrictIp, restrictRange, [roleName]);

            var result = await _userManager.CreateAsync(account, account.Password!);

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

            // Retrieve the account from the store to ensure all fields are up-to-date
            var user = await _userManager.FindByNameAsync(account.UserName!) ?? throw new InvalidOperationException("Account creation failed.");

            // Assign the role to the account

            var createOrUpdateRoleResult = await CreateOrUpdateRoleAsync(roleName);

            if (!createOrUpdateRoleResult.Succeeded)
                throw new InvalidOperationException(string.Join("; ", createOrUpdateRoleResult.Errors.Select(e => e.Description)));

            var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);

            if (!addToRoleResult.Succeeded)
                throw new InvalidOperationException(string.Join("; ", addToRoleResult.Errors.Select(e => e.Description)));

            return user;
        }

        public async Task<Account> GetAccountAsync(Account account, IPAddress remoteIpAddress)
        {
            Account? findAccount = null;

            if (!string.IsNullOrEmpty(account.ApiKey))
            {
                // Search by ApiKey
                findAccount = await _userManager.Users.FirstOrDefaultAsync(a => a.ApiKey == account.ApiKey);
            }
            else if (!string.IsNullOrEmpty(account.UserName) && !string.IsNullOrEmpty(account.Password))
            {
                // Search by username and check password
                findAccount = await _userManager.FindByNameAsync(account.UserName);
                if (findAccount != null)
                {
                    if (!(await _userManager.CheckPasswordAsync(findAccount, account.Password)))
                        findAccount = null;

                }
            }

            if (findAccount == null)
                throw new InvalidOperationException("Invalid credentials or account not found");

            if (!(findAccount.ValidateIpAddress(remoteIpAddress)))
                throw new InvalidOperationException($"Unauthorized request for IP Address {remoteIpAddress.MapToIPv4()}");

            return findAccount;
        }

        public async Task<IList<AccountRole>> GetAccountRoles(Account account)
        {
            ArgumentNullException.ThrowIfNull(nameof(account));

            var roles = await _userManager.GetRolesAsync(account);
            var roleObjects = new List<AccountRole>();
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    roleObjects.Add(role);
                }
            }
            return roleObjects;
        }
    }
}