using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLibrary.Classes
{
    /// <summary>
    /// Represents the possible statuses for a health check result.
    /// </summary>
    /// <remarks>
    /// The <see cref="HealthCheckStatus"/> enum defines the health state of a service, such as unknown, healthy, or unhealthy.
    /// </remarks>
    public enum HealthCheckStatus
    {
        Unknown,
        Healthy,
        Unhealthy
    }

    /// <summary>
    /// Represents the types of services that can be health-checked.
    /// </summary>
    /// <remarks>
    /// The <see cref="HealthCheckService"/> enum specifies which service is being checked, such as none or SQL.
    /// </remarks>
    public enum HealthCheckService
    {
        None,
        Sql
    }

    /// <summary>
    /// Data transfer object representing the result of a health check, including status, message, service, and timestamp.
    /// </summary>
    /// <remarks>
    /// The <see cref="DTOHealthCheckResult"/> class is used to encapsulate health check result details for data transfer scenarios.
    /// </remarks>
    public class DTOHealthCheckResult
    {
        /// <summary>
        /// The status of the health check.
        /// </summary>
        /// <remarks>
        /// Indicates the health state of the checked service, such as Unknown, Healthy, or Unhealthy.
        /// </remarks>
        public HealthCheckStatus Status { get; set; }

        /// <summary>
        /// An optional message providing additional details about the health check result.
        /// </summary>
        /// <remarks>
        /// Can contain error descriptions or contextual information. May be null.
        /// </remarks>
        public string? Message { get; set; }

        /// <summary>
        /// The type of service that was health-checked.
        /// </summary>
        /// <remarks>
        /// Specifies which service was checked, such as None or Sql.
        /// </remarks>
        public HealthCheckService Service { get; set; }

        /// <summary>
        /// The timestamp when the health check was performed.
        /// </summary>
        /// <remarks>
        /// Represents the UTC date and time of the health check result.
        /// </remarks>
        public DateTime Timestamp { get; set; }

    }

    /// <summary>
    /// Represents a persisted health check result entity with identity and conversion to a DTO.
    /// </summary>
    /// <remarks>
    /// The <see cref="HealthCheckResult"/> class extends <see cref="DTOHealthCheckResult"/> by adding a unique identifier for database storage.
    /// It provides a method to convert itself to a <see cref="DTOHealthCheckResult"/> for data transfer purposes.
    /// </remarks>
    public class HealthCheckResult : DTOHealthCheckResult
    {
        /// <summary>
        /// Primary key for the HealthCheckResult entity.
        /// </summary>
        /// <remarks>
        /// Auto-incremented by the database. Used by Entity Framework Core as the unique identifier.
        /// </remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckResult"/> class with default values.
        /// </summary>
        /// <remarks>
        /// Sets status to Unknown, message to null, timestamp to current UTC time, and service to None.
        /// </remarks>
        public HealthCheckResult() : this(HealthCheckStatus.Unknown, null, DateTime.UtcNow, HealthCheckService.None) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckResult"/> class with specified values.
        /// </summary>
        /// <param name="Status">The health check status.</param>
        /// <param name="Message">The optional message describing the result.</param>
        /// <param name="Timestamp">The timestamp of the health check.</param>
        /// <param name="Service">The type of service checked.</param>
        public HealthCheckResult(HealthCheckStatus Status, string? Message, DateTime Timestamp, HealthCheckService Service)
        {
            this.Status = Status;
            this.Message = Message;
            this.Timestamp = Timestamp;
            this.Service = Service;
        }

        /// <summary>
        /// Converts this <see cref="HealthCheckResult"/> entity to a <see cref="DTOHealthCheckResult"/> data transfer object.
        /// </summary>
        /// <remarks>
        /// Copies the status, message, timestamp, and service properties to a new <see cref="DTOHealthCheckResult"/> instance for use in data transfer scenarios.
        /// </remarks>
        /// <returns>A <see cref="DTOHealthCheckResult"/> containing the health check result details.</returns>
        public DTOHealthCheckResult GetDTO()
        {
            return new DTOHealthCheckResult
            {
                Status = Status,
                Message = Message,
                Timestamp = Timestamp,
                Service = Service
            };
        }
    }
}