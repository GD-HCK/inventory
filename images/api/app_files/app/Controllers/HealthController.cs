using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    /// <summary>
    /// Provides endpoints for checking the health status of application services.
    /// </summary>
    /// <remarks>
    /// The <see cref="HealthController"/> class exposes an API endpoint to retrieve health check results for one or more services.
    /// It leverages <see cref="IHealthCheckerContextHelper"/> to perform health checks and returns the results in a standardized format.
    /// </remarks>
    [ApiController] // marks the controller behaviour for Api
    [Route("[controller]")] // sets the api  route to match the name of the class without Controller so /weatherforecast
    [AllowAnonymous]
    public class HealthController(IHealthCheckerContextHelper healthChecker) : ControllerBase
    {
        private readonly IHealthCheckerContextHelper _healthChecker = healthChecker;

        /// <summary>
        /// Retrieves the health status of all services or specific services if provided.
        /// </summary>
        /// <param name="services">An optional list of <see cref="HealthCheckService"/> values to check. If not provided, checks all default services.</param>
        /// <returns>
        /// A list of <see cref="DTOHealthCheckResult"/> objects representing the health status of the requested services.
        /// Returns HTTP 200 if all services are healthy, or HTTP 500 if any service is unhealthy.
        /// </returns>
        /// <remarks>
        /// Sample requests:
        ///
        ///     GET /health
        ///     Content-Type: application/json
        ///
        ///     GET /health
        ///     Content-Type: application/json
        ///     [
        ///         "sql"
        ///     ]
        ///
        /// </remarks>
        /// <response code="200">All services are healthy.</response>
        /// <response code="400">A provided service is invalid.</response>
        /// <response code="500">One or more services are unhealthy.</response>
        [HttpGet(Name = "GetHealth")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IList<DTOHealthCheckResult>>> GetAsync([FromBody] IList<HealthCheckService>? services)
        {
            if (services == null || services.Count == 0)
            {
                services = [HealthCheckService.Sql];
            }

            IList<DTOHealthCheckResult> healthChecks = await _healthChecker.CheckHealthAsync(services);

            bool unhealthy_service = false;

            foreach (var check in healthChecks)
            {
                if (check.Status == HealthCheckStatus.Unhealthy)
                    unhealthy_service = true;
            }

            if (!unhealthy_service)
            {
                return Ok(healthChecks);
            }
            else
            {
                return new JsonResult(healthChecks)
                {
                    ContentType = "application/json",
                    StatusCode = 500
                };
            }
        }
    }
}
