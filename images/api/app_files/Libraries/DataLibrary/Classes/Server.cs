using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLibrary.Classes
{

    /// <summary>
    /// Data transfer object representing a server, including its identity, creation date, name, IP address, scopes, and operating system.
    /// </summary>
    /// <remarks>
    /// The <see cref="DTOServer"/> class is used to encapsulate server details for data transfer scenarios, such as API responses.
    /// It includes basic server properties and supports lists of scopes and an operating system object.
    /// </remarks>
    public class DTOServer
    {
        /// <summary>
        /// Gets or sets the unique identifier for the server.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when the server was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the server.
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of scopes associated with the server.
        /// </summary>
        public IList<DTOServerScope>? Scopes { get; set; }

        /// <summary>
        /// Gets or sets the operating system information for the server.
        /// </summary>
        public DTOServerOS? OS { get; set; }

    }

    /// <summary>
    /// Represents a persisted server entity with identity, configuration, and conversion to a DTO.
    /// </summary>
    /// <remarks>
    /// The <see cref="Server"/> class defines the structure for storing server information in the database, including name, IP address, scopes, and operating system.
    /// It provides constructors for initialization and a method to convert itself to a <see cref="DTOServer"/> for data transfer purposes.
    /// </remarks>
    public class Server
    {
        /// <summary>
        /// Gets or sets the unique identifier for the server.
        /// </summary>
        /// <remarks>
        /// Auto-incremented by the database. Used by Entity Framework Core as the primary key.
        /// </remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when the server was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the server.
        /// </summary>
        public required string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of scopes associated with the server.
        /// </summary>
        public required IList<ServerScope> Scopes { get; set; }

        /// <summary>
        /// Gets or sets the operating system information for the server.
        /// </summary>
        public required ServerOS OS { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        public Server() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class with the specified name, IP address, scopes, and operating system.
        /// </summary>
        /// <param name="Name">The name of the server.</param>
        /// <param name="IPAddress">The IP address of the server.</param>
        /// <param name="Scopes">The list of scopes associated with the server.</param>
        /// <param name="OS">The operating system information for the server.</param>
        public Server(string Name, string IPAddress, IList<ServerScope> Scopes, ServerOS OS)
        {
            this.Name = Name;
            this.IPAddress = IPAddress;
            this.Scopes = Scopes;
            this.OS = OS;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class with the specified id, creation date, name, IP address, scopes, and operating system.
        /// </summary>
        /// <param name="Id">The unique identifier for the server.</param>
        /// <param name="CreatedAt">The UTC date and time when the server was created.</param>
        /// <param name="Name">The name of the server.</param>
        /// <param name="IPAddress">The IP address of the server.</param>
        /// <param name="Scopes">The list of scopes associated with the server.</param>
        /// <param name="OS">The operating system information for the server.</param>
        public Server(int Id, DateTime CreatedAt, string Name, string IPAddress, IList<ServerScope> Scopes, ServerOS OS)
        {
            this.Id = Id;
            this.CreatedAt = CreatedAt;
            this.Name = Name;
            this.IPAddress = IPAddress;
            this.Scopes = Scopes;
            this.OS = OS;
        }

        /// <summary>
        /// Converts this <see cref="Server"/> entity to a <see cref="DTOServer"/> data transfer object.
        /// </summary>
        /// <remarks>
        /// Copies the server's identity, creation date, name, IP address, scopes, and operating system to a new <see cref="DTOServer"/> instance for use in data transfer scenarios.
        /// </remarks>
        /// <returns>A <see cref="DTOServer"/> containing the server details.</returns>
        public DTOServer GetDTO()
        {
            return new DTOServer
            {
                Id = Id,
                CreatedAt = CreatedAt,
                Name = Name,
                IPAddress = IPAddress,
                Scopes = Scopes?.Select(s => s.GetDTO()).ToList(),
                OS = OS.GetDTO()
            };
        }
    }
}
