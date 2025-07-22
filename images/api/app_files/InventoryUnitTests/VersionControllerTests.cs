using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Inventory.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InventoryUnitTests
{
    public class VersionControllerTests
    {

        // GET

        [Fact]
        public void GetVersion_Returns200()
        {
            var mockVersionHelper = new Mock<IApiVersionContextHelper>();
            var controller = new VersionController(mockVersionHelper.Object);

            mockVersionHelper
                .Setup(x => x.GetLatestVersion())
                .Returns(
                    new ApiVersion
                    {
                        Id = 1,
                        Major = 1,
                        Minor = 0,
                        Patch = 0,
                        BuildDate = DateTime.UtcNow,
                        GitCommit = "abc123",
                        GitBranch = "main",
                        Platform = "container"
                    }.GetDTO()
                );

            ActionResult<DTOApiVersion> result;

            result = controller.GetVersion();
            var createdResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<DTOApiVersion>(createdResult.Value);
        }
    }
}
