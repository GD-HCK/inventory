namespace DataLibrary.Classes
{
    /// <summary>
    /// Represents a data transfer object for updating one or more properties of a server entity.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServerPatch"/> class is used to encapsulate partial updates to a server, such as name, IP address, scopes, or operating system.
    /// It supports both parameterless and parameterized constructors for flexible initialization in patch/update scenarios.
    /// </remarks>
    public class ServerPatch(int Id)
    {
        /// <summary>
        /// Gets or sets the unique identifier of the server to be updated.
        /// </summary>
        public required int Id { get; set; } = Id;

        /// <summary>
        /// Gets or sets the UTC date and time when the patch object was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the new name for the server, if being updated.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the new IP address for the server, if being updated.
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the new list of scopes for the server, if being updated.
        /// </summary>
        public IList<ServerScope>? Scopes { get; set; }

        /// <summary>
        /// Gets or sets the new operating system information for the server, if being updated.
        /// </summary>
        public ServerOS? OS { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerPatch"/> class with default values.
        /// </summary>
        public ServerPatch() : this(0) { }
    }
}
