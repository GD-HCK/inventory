using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Represents a set of permissions for a specific API endpoint, including allowed actions and role association.
    /// </summary>
    /// <remarks>
    /// The <see cref="EndpointPermission"/> class defines which actions are permitted on a given API endpoint for a particular role.
    /// It includes a list of allowed actions, a reference to the associated <see cref="AccountRole"/>, and is designed for use with Entity Framework Core.
    /// </remarks>
    public class EndpointPermission
    {
        /// <summary>
        /// Primary key for the EndpointPermission entity.
        /// </summary>
        /// <remarks>
        /// Auto-incremented by the database. Used by Entity Framework Core as the unique identifier.
        /// </remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Primary key for EF Core

        /// <summary>
        /// The API endpoint to which these permissions apply.
        /// </summary>
        /// <remarks>
        /// Example: "health".
        /// </remarks>
        public string? Endpoint { get; set; }
        /// <summary>
        /// The list of allowed actions for this endpoint.
        /// </summary>
        /// <remarks>
        /// Contains one or more <see cref="EndpointPermissionAction"/> values (e.g., Read, Write).
        /// </remarks>
        public IList<EndpointPermissionAction>? EndpointPermissionActions { get; set; }

        /// <summary>
        /// The foreign key identifier for the associated AccountRole.
        /// </summary>
        /// <remarks>
        /// Nullable. Links this permission set to a specific role.
        /// </remarks>
        public string? AccountRoleId { get; set; }

        /// <summary>
        /// The navigation property to the associated <see cref="AccountRole"/> entity.
        /// </summary>
        /// <remarks>
        /// Nullable. Provides access to the related <see cref="AccountRole"/> object.
        /// </remarks>
        public AccountRole? AccountRole { get; set; }

        public EndpointPermission() { }

        public EndpointPermission(string endpoint, IList<EndpointPermissionAction> endpointPermissionActions)
        {
            Endpoint = endpoint;
            EndpointPermissionActions = endpointPermissionActions;
        }
    }
}
