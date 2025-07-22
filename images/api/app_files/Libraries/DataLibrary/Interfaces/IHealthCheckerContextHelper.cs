using DataLibrary.Classes;

namespace DataLibrary.Interfaces
{
    /// <summary>
    /// Defines methods for performing health checks on application services.
    /// </summary>
    /// <remarks>
    /// The <see cref="IHealthCheckerContextHelper"/> interface provides a contract for running health checks on one or more services and returning their status results.
    /// </remarks>
    public interface IHealthCheckerContextHelper
    {
        /// <summary>
        /// Runs a health check on a list of <see cref="HealthCheckService"/> services.
        /// </summary>
        /// <param name="healthCheckServices">The list of services to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="DTOHealthCheckResult"/> objects with the health check results.</returns>
        public Task<IList<DTOHealthCheckResult>> CheckHealthAsync(IList<HealthCheckService> healthCheckServices);
    }
}