using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Enumerates the supported scopes for a server, representing the roles or services a server can provide.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServerScopeType"/> enum defines the possible scopes for a server, such as web, database, file storage, SMTP, SFTP, hypervisor, SQL, DNS, ad blocker, Docker, Kubernetes, Jenkins, Redis, or none.
    /// This is used to classify and manage the functional roles of servers throughout the application.
    /// </remarks>
    public enum ServerScopeType
    {
        /// <summary>
        /// The server provides web hosting or web application services.
        /// </summary>
        web,

        /// <summary>
        /// The server provides database services.
        /// </summary>
        database,

        /// <summary>
        /// The server provides file storage services.
        /// </summary>
        filestorage,

        /// <summary>
        /// The server provides SMTP (email sending) services.
        /// </summary>
        smtp,

        /// <summary>
        /// The server provides SFTP (Secure File Transfer Protocol) services.
        /// </summary>
        sftp,

        /// <summary>
        /// The server acts as a hypervisor for virtual machines.
        /// </summary>
        hypervisor,

        /// <summary>
        /// The server provides SQL database services.
        /// </summary>
        sql,

        /// <summary>
        /// The server provides DNS (Domain Name System) services.
        /// </summary>
        dns,

        /// <summary>
        /// The server provides ad blocking services.
        /// </summary>
        adblocker,

        /// <summary>
        /// The server hosts Docker containers.
        /// </summary>
        docker,

        /// <summary>
        /// The server runs Kubernetes for container orchestration.
        /// </summary>
        kubernetes,

        /// <summary>
        /// The server runs Jenkins for continuous integration/continuous deployment (CI/CD).
        /// </summary>
        jenkins,

        /// <summary>
        /// The server provides Redis in-memory data store services.
        /// </summary>
        redis,

        /// <summary>
        /// The server does not provide any specific scope or its scope is undefined.
        /// </summary>
        none
    }


    /// <summary>
    /// Data transfer object representing a server's scope.
    /// </summary>
    /// <remarks>
    /// The <see cref="DTOServerScope"/> class is used to encapsulate the scope information for a server in data transfer scenarios.
    /// </remarks>
    public class DTOServerScope
    {
        /// <summary>
        /// Gets or sets the scope of the server.
        /// </summary>
        /// <remarks>
        /// Specifies the type of scope, such as web, database, file storage, SMTP, SFTP, hypervisor, SQL, DNS, ad blocker, Docker, Kubernetes, Jenkins, Redis, or none.
        /// </remarks>
        public ServerScopeType ScopeType { get; set; }
    }

    /// <summary>
    /// Represents a persisted server scope entity with identity, server association, and conversion to a DTO.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServerScope"/> class extends <see cref="DTOServerScope"/> by adding a unique identifier and a reference to the associated <see cref="Server"/>.
    /// It provides a method to convert itself to a <see cref="DTOServerScope"/> for data transfer purposes.
    /// </remarks>
    public class ServerScope : DTOServerScope
    {
        /// <summary>
        /// Gets or sets the unique identifier for the server scope entity.
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
        /// Initializes a new instance of the <see cref="ServerScope"/> class with the specified scope.
        /// </summary>
        /// <param name="ScopeType">The server's scope.</param>
        public ServerScope(ServerScopeType ScopeType)
        {
            this.ScopeType = ScopeType;
        }

        /// <summary>
        /// Converts this <see cref="ServerScope"/> entity to a <see cref="DTOServerScope"/> data transfer object.
        /// </summary>
        /// <remarks>
        /// Copies the scope property to a new <see cref="DTOServerScope"/> instance for use in data transfer scenarios.
        /// </remarks>
        /// <returns>A <see cref="DTOServerScope"/> containing the scope information.</returns>
        public DTOServerScope GetDTO()
        {
            return new DTOServerScope
            {
                ScopeType = ScopeType
            };
        }
    }
}
