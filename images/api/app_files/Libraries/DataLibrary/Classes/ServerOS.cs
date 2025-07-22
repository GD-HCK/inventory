using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Enumerates the supported operating systems for a server.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServerOSType"/> enum defines the possible operating systems that a server can have,
    /// such as Windows, Linux, macOS, Unix, or other. This is used for classifying and managing server OS types
    /// throughout the application, including data transfer and persistence scenarios.
    /// </remarks>
    public enum ServerOSType
    {
        /// <summary>
        /// The server is running Microsoft Windows.
        /// </summary>
        windows,

        /// <summary>
        /// The server is running a Linux-based operating system.
        /// </summary>
        linux,

        /// <summary>
        /// The server is running macOS.
        /// </summary>
        macos,

        /// <summary>
        /// The server is running a Unix-based operating system (other than Linux or macOS).
        /// </summary>
        unix,

        /// <summary>
        /// The server is running an operating system not covered by the other options.
        /// </summary>
        other
    }

    /// <summary>
    /// Data transfer object representing a server's operating system.
    /// </summary>
    /// <remarks>
    /// The <see cref="DTOServerOS"/> class is used to encapsulate the operating system information for a server in data transfer scenarios.
    /// </remarks>
    public class DTOServerOS
    {
        /// <summary>
        /// Gets or sets the operating system of the server.
        /// </summary>
        /// <remarks>
        /// Specifies the type of operating system, such as Windows, Linux, macOS, Unix, or other.
        /// </remarks>
        public ServerOSType ServerOSType { get; set; }
    }

    /// <summary>
    /// Represents a persisted server operating system entity with identity, server association, and conversion to a DTO.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServerOS"/> class extends <see cref="DTOServerOS"/> by adding a unique identifier and a reference to the associated <see cref="Server"/>.
    /// It provides a method to convert itself to a <see cref="DTOServerOS"/> for data transfer purposes.
    /// </remarks>
    public class ServerOS : DTOServerOS
    {
        /// <summary>
        /// Gets or sets the unique identifier for the server operating system entity.
        /// </summary>
        /// <remarks>
        /// Auto-incremented by the database. Used by Entity Framework Core as the primary key.
        /// </remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key identifier for the associated server.
        /// </summary>
        public int ServerId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the associated <see cref="Server"/> entity.
        /// </summary>
        /// <remarks>
        /// Provides access to the related server object. Ignored during JSON serialization.
        /// </remarks>
        [ForeignKey("ServerId")]
        [JsonIgnore]
        public Server? Server { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerOS"/> class with the specified operating system.
        /// </summary>
        /// <param name="ServerOSType">The server's operating system.</param>
        public ServerOS(ServerOSType ServerOSType)
        {
            this.ServerOSType = ServerOSType;
        }

        /// <summary>
        /// Converts this <see cref="ServerOS"/> entity to a <see cref="DTOServerOS"/> data transfer object.
        /// </summary>
        /// <remarks>
        /// Copies the operating system property to a new <see cref="DTOServerOS"/> instance for use in data transfer scenarios.
        /// </remarks>
        /// <returns>A <see cref="DTOServerOS"/> containing the operating system information.</returns>
        public DTOServerOS GetDTO()
        {
            return new DTOServerOS
            {
                ServerOSType = ServerOSType
            };
        }
    }
}
