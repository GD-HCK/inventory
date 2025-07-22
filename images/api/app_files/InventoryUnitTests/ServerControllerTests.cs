using DataLibrary.Classes;
using DataLibrary.Contexts;
using DataLibrary.Helpers;
using DataLibrary.Interfaces;
using Inventory.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Reflection;

namespace InventoryUnitTests
{
    public class ServerControllerTests
    {

        // ServerController tests

        // GET
        [Fact]
        public async Task GetByIdAsync_WithValidId_Returns404()
        {
            // Setup in-memory DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("GetByIdAsync_WithValidId_Returns404")
                .Options;

            var dbContext = new AppDbContext(options);

            var helper = new ServerContextHelper(dbContext);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await helper.GetServerByIdAsync(1));
        }

        [Theory]
        [InlineData(1, typeof(OkObjectResult))]      // Valid ID
        [InlineData(0, typeof(BadRequestObjectResult))] // Invalid ID
        public async Task GetByIdAsync_VariousIds_ReturnsExpectedResultType(int id, Type expectedResultTypeType)
        {
            var mockServerContextHelper = new Mock<IServerContextHelper>();
            var controller = new ServerController(mockServerContextHelper.Object);

            if (expectedResultTypeType == typeof(OkObjectResult))
            {
                var server = new Server
                {
                    CreatedAt = DateTime.UtcNow,
                    Id = 1,
                    IPAddress = "1.1.1.1",
                    Name = "svr_test_1",
                    Scopes = [ new (ServerScopeType.web) {
                        Id = 1,
                        ServerId = 1
                    }],
                    OS = new(ServerOSType.windows)
                    {
                        Id = 1,
                        ServerId = 1
                    }
                };
                mockServerContextHelper.Setup(m => m.GetServerByIdAsync(id)).ReturnsAsync(server.GetDTO());
                var result = await controller.GetByIdAsync(id);

                var resultCheck = Assert.IsType<OkObjectResult>(result.Result);
                Assert.IsType<DTOServer>(resultCheck.Value);
            }
            else
            {
                var result = await controller.GetByIdAsync(id);

                Assert.IsType(expectedResultTypeType, result.Result);
            }
        }

        [Fact]
        public void GetByFilter_WithValidFilters_Returns200()
        {
            var mockServerContextHelper = new Mock<IServerContextHelper>();

            var controller = new ServerController(mockServerContextHelper.Object);

            var server1 = new Server
            {
                CreatedAt = DateTime.UtcNow,
                Id = 1,
                IPAddress = "1.1.1.1",
                Name = "svr_test_1",
                Scopes = [new (ServerScopeType.web){
                        Id = 1,
                        ServerId = 1
                    }],
                OS = new(ServerOSType.windows)
                {
                    Id = 1,
                    ServerId = 1
                }
            };

            var server2 = new Server
            {
                CreatedAt = DateTime.UtcNow,
                Id = 2,
                IPAddress = "1.1.1.2",
                Name = "svr_test_2",
                Scopes = [new (ServerScopeType.web)
                    {
                        Id = 2,
                        ServerId = 2
                    }],
                OS = new(ServerOSType.windows)
                {
                    Id = 2,
                    ServerId = 2
                }
            };

            var serversList = new List<DTOServer>
            {
                server1.GetDTO(),
                server2.GetDTO()
            };

            var paginatedList = new PaginatedList<DTOServer>(serversList.AsQueryable(), 1, 2)
            {
                Links = new Links()
                {
                    Next = null,
                    Previous = null
                },
                TotalCount = serversList.Count,
                TotalPages = 1,
                Results = serversList
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Host = new HostString("inventory.example.com");
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Path = "/server";
            controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?page=1&limit=2&name=svr");

            var filterDic = new Dictionary<string, string>
            {
                { "page", "1" },
                { "limit", "2"},
                { "name", "svr" }
            };

            mockServerContextHelper.Setup(m => m.GetServers(filterDic))
                .Returns(paginatedList);

            var resultList = controller.GetByFilter(filterDic);

            var okResult = Assert.IsType<OkObjectResult>(resultList);
            var verify = Assert.IsType<PaginatedList<DTOServer>>(okResult.Value);
        }

        [Fact]
        public void GetByFilter_WithInvalidFilters_Returns500()
        {
            // Setup in-memory DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("GetByFilter_WithInvalidFilters_Returns500")
                .Options;

            var dbContext = new AppDbContext(options);

            var helper = new ServerContextHelper(dbContext);

            var invalidFilter = new Dictionary<string, string>
            {
                { "test", "1" }
            };

            // Act & Assert
            var ex = Assert.Throws<InvalidFilterCriteriaException>(() => helper.GetServers(invalidFilter));
            Assert.Contains("test", ex.Message);
        }

        // POST

        [Fact]
        public async Task PostAsync_WithValidServer_Returns201()
        {
            var mockServerContextHelper = new Mock<IServerContextHelper>();

            var controller = new ServerController(mockServerContextHelper.Object);

            var postServer = new Server
            {
                CreatedAt = DateTime.UtcNow,
                IPAddress = "1.1.1.1",
                Name = "svr_test_1",
                Scopes = [
                    new (ServerScopeType.web)
                ],
                OS = new(ServerOSType.windows)
            };

            var server = new Server
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                IPAddress = "1.1.1.1",
                Name = "svr_test_1",
                Scopes = [
                    new (ServerScopeType.web)
                    {
                        Id = 1,
                        ServerId = 1
                    }
                ],
                OS = new(ServerOSType.windows)
                {
                    Id = 1,
                    ServerId = 1
                }
            };

            mockServerContextHelper.Setup(m => m.PostServerAsync(postServer))
                .Returns(Task.FromResult(server.GetDTO()));

            var resultList = await controller.PostAsync(postServer);

            var createdResult = Assert.IsType<CreatedAtActionResult>(resultList.Result);
            var verify = Assert.IsType<DTOServer>(createdResult.Value);
        }

        // PATCH

        [Fact]
        public async Task PatchAsync_WithValidIdAndServerPatch_Returns404()
        {
            // Setup in-memory DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("PatchAsync_Assert404")
                .Options;

            var dbContext = new AppDbContext(options);

            var helper = new ServerContextHelper(dbContext);

            var patchServer = new ServerPatch
            {
                Id = 1,
                IPAddress = "1.1.1.2"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await helper.PatchServerAsync(patchServer));
        }

        [Theory]
        [InlineData(1, 1, typeof(OkObjectResult))]          // Valid ID
        [InlineData(0, 0, typeof(BadRequestObjectResult))]  // Zero ID
        [InlineData(1, 2, typeof(BadRequestObjectResult))]  // ID mismatch
        public async Task PatchAsync_WithDifferentParameters_ReturnsExpectedRusult(int queryId, int bodyId, Type expectedResultType)
        {
            var mockServerContextHelper = new Mock<IServerContextHelper>();

            var controller = new ServerController(mockServerContextHelper.Object);

            var patchServer = new ServerPatch
            {
                Id = bodyId,
                IPAddress = "1.1.1.2"
            };

            if (expectedResultType == typeof(OkObjectResult))
            {
                var server = new Server
                {
                    Id = 1,
                    CreatedAt = DateTime.UtcNow,
                    IPAddress = "1.1.1.2",
                    Name = "svr_test_1",
                    Scopes = [
                    new (ServerScopeType.web)
                    {
                        Id = 1,
                        ServerId = 1
                    }
                ],
                    OS = new(ServerOSType.windows)
                    {
                        Id = 1,
                        ServerId = 1
                    }
                };

                mockServerContextHelper.Setup(m => m.PatchServerAsync(patchServer))
                .Returns(Task.FromResult(server.GetDTO()));

                var resultList = await controller.PatchAsync(queryId, patchServer);

                var resultCheck = Assert.IsType<OkObjectResult>(resultList.Result);

                Assert.IsType<DTOServer>(resultCheck.Value);

            }
            else
            {
                var resultList = await controller.PatchAsync(queryId, patchServer);
                Assert.IsType(expectedResultType, resultList.Result);
            }
        }


        // PUT

        [Fact]
        public async Task PutAsync_WithValidIdAndServerPatch_Returns404()
        {
            // Setup in-memory DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("PutAsync_WithValidIdAndServerPatch_Returns404")
                .Options;

            var dbContext = new AppDbContext(options);

            var helper = new ServerContextHelper(dbContext);

            var server = new Server
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                IPAddress = "1.1.1.2",
                Name = "svr_test_1",
                Scopes = [
                    new (ServerScopeType.web)
                    {
                        Id = 1,
                        ServerId = 1
                    }
                ],
                OS = new(ServerOSType.windows)
                {
                    Id = 1,
                    ServerId = 1
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await helper.PutServerAsync(server));
        }

        [Theory]
        [InlineData(1, 1, typeof(OkObjectResult))]          // Valid ID
        [InlineData(0, 0, typeof(BadRequestObjectResult))]  // Zero ID
        [InlineData(1, 2, typeof(BadRequestObjectResult))]  // ID mismatch
        public async Task PutAsync_WithDifferentParameters_ReturnsExpectedRusult(int queryId, int bodyId, Type expectedResultType)
        {
            var mockServerContextHelper = new Mock<IServerContextHelper>();

            var controller = new ServerController(mockServerContextHelper.Object);

            var server = new Server
            {
                Id = bodyId,
                CreatedAt = DateTime.UtcNow,
                IPAddress = "1.1.1.2",
                Name = "svr_test_1",
                Scopes = [
                    new (ServerScopeType.web)
                    {
                        Id = 1,
                        ServerId = bodyId
                    }
                ],
                OS = new(ServerOSType.windows)
                {
                    Id = 1,
                    ServerId = bodyId
                }
            };

            mockServerContextHelper.Setup(m => m.PutServerAsync(server))
            .Returns(Task.FromResult(server.GetDTO()));

            if (expectedResultType == typeof(OkObjectResult))
            {
                var resultList = await controller.PutAsync(queryId, server);
                var resultCheck = Assert.IsType<OkObjectResult>(resultList.Result);
                Assert.IsType<DTOServer>(resultCheck.Value);
            }
            else
            {
                var resultList = await controller.PutAsync(queryId, server);
                Assert.IsType(expectedResultType, resultList.Result);
            }
        }


        // DELETE

        [Fact]
        public async Task DeleteAsync_WithValidId_Returns404()
        {
            // Setup in-memory DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("DeleteAsync_WithValidId_Returns404")
                .Options;

            var dbContext = new AppDbContext(options);

            var helper = new ServerContextHelper(dbContext);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await helper.DeleteServerAsync(1));
        }

        [Theory]
        [InlineData(1, typeof(OkObjectResult))]          // Valid ID
        [InlineData(0, typeof(BadRequestObjectResult))]  // Zero ID
        public async Task DeleteAsync_WithVariousIds_ReturnsExpectedResultType(int id, Type expectedResultType)
        {
            var mockServerContextHelper = new Mock<IServerContextHelper>();

            var controller = new ServerController(mockServerContextHelper.Object);

            switch (expectedResultType)
            {
                case Type t when t == typeof(NoContentResult):

                    mockServerContextHelper.Setup(m => m.DeleteServerAsync(id))
                        .ReturnsAsync(id);

                    break;

                case Type t when t == typeof(BadRequestObjectResult):

                    mockServerContextHelper.Setup(m => m.DeleteServerAsync(id))
                        .Throws(new ArgumentException($"Invalid server Id ${id}."));

                    break;
            }

            var result = await controller.DeleteAsync(id);
            Assert.IsType(expectedResultType, result);
        }
    }
}
