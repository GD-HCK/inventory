using DataLibrary.Classes;
using DataLibrary.Contexts;
using DataLibrary.Helpers;
using DataLibrary.Interfaces;
using Inventory.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InventoryUnitTests
{
    public class HealthControllerTests
    {

        // GET

        [Theory]
        [InlineData(HealthCheckService.Sql)]
        [InlineData(null)]
        public async Task GetAsync_WithVariousServices_ReturnsExpectedResultType(HealthCheckService? healthCheckService)
        {
            var mockHealthCheckerHelper = new Mock<IHealthCheckerContextHelper>();
            var controller = new HealthController(mockHealthCheckerHelper.Object);

            mockHealthCheckerHelper
                .Setup(x => x.CheckHealthAsync(It.IsAny<IList<HealthCheckService>>()))
                .ReturnsAsync(
                [
                    new DTOHealthCheckResult
                    {
                        Status = HealthCheckStatus.Healthy,
                        Service = HealthCheckService.Sql,
                        Timestamp = DateTime.Now
                    }
                ]);

            ActionResult<IList<DTOHealthCheckResult>> result;

            if (healthCheckService != null)
            {
                var list = new List<HealthCheckService>
                {
                    healthCheckService!.Value
                };

                result = await controller.GetAsync(list);
                var createdResult = Assert.IsType<OkObjectResult>(result.Result);
                Assert.IsType<List<DTOHealthCheckResult>>(createdResult.Value);
            }
            else
            {
                result = await controller.GetAsync(null);
            }
        }


        [Fact]
        public async Task GetAsync_WithNotImplementedService_Returns500()
        {
            // Setup in-memory DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("GetAsync_WithNotImplementedService_Returns500")
                .Options;

            var dbContext = new AppDbContext(options);

            // Use the real HealthCheckerHelper
            var healthCheckerHelper = new HealthCheckerContextHelper(dbContext);
            var controller = new HealthController(healthCheckerHelper);

            var list = new List<HealthCheckService>
            {
                HealthCheckService.None // This should trigger NotImplementedException
            };

            await Assert.ThrowsAsync<NotImplementedException>(() => controller.GetAsync(list));
        }

    }
}
