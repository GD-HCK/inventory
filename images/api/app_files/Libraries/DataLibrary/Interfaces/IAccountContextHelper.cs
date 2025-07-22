using DataLibrary.Classes;
using System.Net;

namespace DataLibrary.Interfaces
{
    /// <summary>
    /// Defines methods for managing user accounts and roles, including creation, authentication, and role retrieval.
    /// </summary>
    /// <remarks>
    /// The <see cref="IAccountContextHelper"/> interface provides a contract for account management operations such as creating accounts with role and IP restrictions, authenticating users, retrieving assigned roles, and looking up roles by name.
    /// </remarks>
    public interface IAccountContextHelper
    {
        /// <summary>
        /// Creates a new <see cref="Account"/> with the specified role and IP restrictions, and assigns the role to the account.
        /// </summary>
        /// <param name="ipAddress">The IP address to restrict, or null for no restriction.</param>
        /// <param name="restrictIp">Whether to restrict to a single IP address.</param>
        /// <param name="restrictRange">Whether to restrict to an IP range.</param>
        /// <param name="roleType">The type of role to assign to the account.</param>
        /// <returns>The created <see cref="Account"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if account creation or role assignment fails.</exception>
        public Task<Account> AddAccountAsync(IPAddress? ipAddress, bool restrictIp, bool restrictRange, RoleType roleType);

        /// <summary>
        /// Retrieves an <see cref="Account"/> by credentials and validates the remote IP address.
        /// </summary>
        /// <param name="account">The account credentials to validate.</param>
        /// <param name="remoteIpAddress">The remote IP address to validate against allowed IPs.</param>
        /// <returns>The authenticated <see cref="Account"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if credentials are invalid or IP is not allowed.</exception>
        public Task<Account> GetAccountAsync(Account account, IPAddress remoteIpAddress);

        /// <summary>
        /// Retrieves the list of <see cref="AccountRole"/> objects assigned to the specified account.
        /// </summary>
        /// <param name="account">The account for which to retrieve roles.</param>
        /// <returns>A list of <see cref="AccountRole"/> objects assigned to the account.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="account"/> is null.</exception>
        public Task<IList<AccountRole>> GetAccountRoles(Account account);

        /// <summary>
        /// Retrieves an <see cref="AccountRole"/> by its name, including endpoint permissions and actions.
        /// </summary>
        /// <param name="roleName">The name of the role to retrieve.</param>
        /// <returns>The <see cref="AccountRole"/> with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="roleName"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the role is not found.</exception>
        public Task<AccountRole> GetRoleByNameAsync(string roleName);
    }
}
