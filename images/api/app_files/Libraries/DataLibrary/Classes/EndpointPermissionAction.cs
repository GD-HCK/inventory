using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Enumerates the possible actions that can be permitted on an API endpoint.
    /// </summary>
    /// <remarks>
    /// The <see cref="EndpointPermissionActionType"/> enum defines the set of actions that can be allowed for a specific API endpoint permission,
    /// supporting fine-grained access control. Typical actions include Read, Write, Delete, Update, and Create.
    /// </remarks>
    public enum EndpointPermissionActionType
    {
        /// <summary>
        /// Grants permission to read data from the endpoint.
        /// </summary>
        Read,

        /// <summary>
        /// Grants permission to delete data from the endpoint.
        /// </summary>
        Delete,

        /// <summary>
        /// Grants permission to update existing data at the endpoint.
        /// </summary>
        Update,

        /// <summary>
        /// Grants permission to create new data at the endpoint.
        /// </summary>
        Create,

        /// <summary>
        /// Grants permission to create, update and delete data at the endpoint.
        /// </summary>
        Write,

        /// <summary>
        /// Grants no permission for the endpoint.
        /// </summary>
        None

    }

    /// <summary>
    /// Represents an allowed action for a specific API endpoint permission.
    /// </summary>
    /// <remarks>
    /// The <see cref="EndpointPermissionAction"/> class defines an individual action (such as Read, Write, Delete, Update, or Create)
    /// that is permitted on an API endpoint. Each instance is associated with a specific <see cref="EndpointPermission"/> entity,
    /// allowing for fine-grained access control at the endpoint level. This class is designed for use with Entity Framework Core,
    /// supporting database identity, foreign key relationships, and navigation properties.
    /// </remarks>
    public class EndpointPermissionAction(EndpointPermissionActionType EndpointPermissionActionType)
    {
        /// <summary>
        /// Primary key for the EndpointPermissionAction entity.
        /// </summary>
        /// <remarks>
        /// Auto-incremented by the database. Used by Entity Framework Core as the unique identifier.
        /// </remarks>
        [Key] // Attribute for EF Core to recognize this as a primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment primary key
        public int Id { get; set; }

        /// <summary>
        /// The action permitted by this permission action.
        /// </summary>
        /// <remarks>
        /// Specifies the type of action (e.g., Read, Write, Delete, Update, Create) allowed on the endpoint.
        /// This is a required field.
        /// </remarks>
        public EndpointPermissionActionType EndpointPermissionActionType { get; set; } = EndpointPermissionActionType;

        /// <summary>
        /// The foreign key identifier for the associated EndpointPermission.
        /// </summary>
        /// <remarks>
        /// Links this action to a specific EndpointPermission entity.
        /// </remarks>
        public int EndpointPermissionId { get; set; }

        /// <summary>
        /// The navigation property to the associated EndpointPermission entity.
        /// </summary>
        /// <remarks>
        /// Provides access to the related EndpointPermission object.
        /// </remarks>
        public EndpointPermission? EndpointPermission { get; set; }
    }
}
