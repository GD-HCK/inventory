using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Data transfer object representing API version information, including version numbers, Git metadata, build date, and platform.
    /// </summary>
    /// <remarks>
    /// The <see cref="DTOApiVersion"/> class is used to encapsulate versioning details for the API, such as major, minor, and patch numbers, as well as optional Git commit and branch information, build date, and platform.
    /// This class is intended for use in data transfer scenarios where version metadata needs to be communicated.
    /// </remarks>
    public class DTOApiVersion
    {
        /// <summary>
        /// Gets or sets the major version number of the API.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Gets or sets the minor version number of the API.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Gets or sets the patch version number of the API.
        /// </summary>
        public int Patch { get; set; }

        /// <summary>
        /// Gets or sets the Git commit hash associated with this API build.
        /// </summary>
        public string? GitCommit { get; set; }

        /// <summary>
        /// Gets or sets the Git branch name associated with this API build.
        /// </summary>
        public string? GitBranch { get; set; }

        /// <summary>
        /// Gets or sets the build date and time (UTC) of the API.
        /// </summary>
        public DateTime BuildDate { get; set; }

        /// <summary>
        /// Gets or sets the platform or environment for which the API was built.
        /// </summary>
        public string? Platform { get; set; }
    }

    /// <summary>
    /// Represents a persisted API version entity with identity and conversion to a DTO.
    /// </summary>
    /// <remarks>
    /// The <see cref="ApiVersion"/> class extends <see cref="DTOApiVersion"/> by adding a unique identifier for database storage.
    /// It provides a method to convert itself to a <see cref="DTOApiVersion"/> for data transfer purposes.
    /// </remarks>
    public class ApiVersion : DTOApiVersion
    {
        /// <summary>
        /// Gets or sets the unique identifier for the API version entity.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        public ApiVersion() { }

        /// <summary>
        /// Converts this <see cref="ApiVersion"/> entity to a <see cref="DTOApiVersion"/> data transfer object.
        /// </summary>
        /// <returns>A <see cref="DTOApiVersion"/> containing the version details.</returns>
        public DTOApiVersion GetDTO()
        {
            return new DTOApiVersion
            {
                Major = Major,
                Minor = Minor,
                Patch = Patch,
                GitCommit = GitCommit,
                GitBranch = GitBranch,
                BuildDate = BuildDate,
                Platform = Platform
            };
        }
    }
}
