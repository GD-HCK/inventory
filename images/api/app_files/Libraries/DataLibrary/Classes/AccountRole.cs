using Microsoft.AspNetCore.Identity;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Represents a valid account role.
    /// </summary>
    public enum RoleType
    {
        Admin,
        Priviledged,
        SingleEndpoint,
        User,
        Guest
    }

    /// <summary>
    /// Represents a user role within the application, including a description and endpoint permissions.
    /// </summary>
    /// <remarks>
    /// The <see cref="AccountRole"/> class extends <see cref="IdentityRole"/> to add a description and a list of endpoint permissions for fine-grained access control.
    /// It supports both parameterless and convenience constructors for use with Entity Framework Core and manual instantiation.
    /// </remarks>
    public class AccountRole : IdentityRole
    {
        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the list of endpoint permissions associated with this role.
        /// </summary>
        public IList<EndpointPermission>? EndpointPermissions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountRole"/> class.
        /// </summary>
        /// <remarks>
        /// This parameterless constructor is required for Entity Framework Core.
        /// </remarks>
        public AccountRole() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountRole"/> class with the specified name, description, and endpoint permissions.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <param name="endpoints">The list of endpoint permissions for the role.</param>

        public AccountRole(string roleName, string description, IList<EndpointPermission> endpoints)
            : base(roleName)
        {
            Description = description;
            EndpointPermissions = endpoints;
        }
    }
}
