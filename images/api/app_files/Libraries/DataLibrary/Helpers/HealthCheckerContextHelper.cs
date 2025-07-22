using DataLibrary.Classes;
using DataLibrary.Contexts;
using DataLibrary.Interfaces;

namespace DataLibrary.Helpers
{
    public class HealthCheckerContextHelper(AppDbContext appDbContext) : IHealthCheckerContextHelper
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<HealthCheckResult> AddHealthCheckResultAsync(HealthCheckResult healthCheckResult)
        {
            _appDbContext.HealthCheckResults.Add(healthCheckResult);

            await _appDbContext.SaveChangesAsync();

            return healthCheckResult;
        }

        public async Task<DTOHealthCheckResult> CheckSqlHealthAsync()
        {
            HealthCheckResult healthCheckResult = new()
            {
                Message = "HealthCheck triggered by endpoint /Health",
                Service = HealthCheckService.Sql,
                Status = HealthCheckStatus.Healthy,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                await AddHealthCheckResultAsync(healthCheckResult);
            }
            catch (Exception ex)
            {
                healthCheckResult.Status = HealthCheckStatus.Unhealthy;
                healthCheckResult.Message = $"SQL Server is not reachable: {ex.Message}";
            }

            return healthCheckResult.GetDTO();

        }

        public async Task<IList<DTOHealthCheckResult>> CheckHealthAsync(IList<HealthCheckService> healthCheckServices)
        {
            List<DTOHealthCheckResult> healthCheckResults = [];

            foreach (var service in healthCheckServices)
            {
                switch (service)
                {
                    case HealthCheckService.Sql:
                        healthCheckResults.Add(await CheckSqlHealthAsync());
                        break;
                    default:
                        throw new NotImplementedException($"Health check for {service} is not implemented");
                }
            }

            return healthCheckResults;
        }
    }
}
