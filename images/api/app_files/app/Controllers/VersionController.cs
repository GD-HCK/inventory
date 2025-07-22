using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    /// <summary>
    /// Provides an endpoint for retrieving the current API version information.
    /// </summary>
    /// <remarks>
    /// The <see cref="VersionController"/> class exposes an API endpoint to return metadata about the current API version, including version numbers, build information, and source control metadata.
    /// It leverages <see cref="IApiVersionContextHelper"/> to access version data.
    /// </remarks>
    [ApiController] // marks the controller behaviour for Api
    [Route("[controller]")] // sets the api  route to match the name of the class without Controller so /weatherforecast
    [AllowAnonymous] // using only [Authorize] will only use the default scheme which is ApiKey.
    public partial class VersionController(IApiVersionContextHelper apiVersionContextHelper) : ControllerBase
    {
        private readonly IApiVersionContextHelper _apiVersionContextHelper = apiVersionContextHelper;

        /// <summary>
        /// Retrieves the current API version information.
        /// </summary>
        /// <returns>An <see cref="ActionResult{DTOApiVersion}"/> containing the API version details.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /version
        ///
        /// </remarks>
        /// <response code="200">Returns the current API version information.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<DTOApiVersion> GetVersion()
        {
            var version = _apiVersionContextHelper.GetLatestVersion();

            return Ok(version);
        }
    }
}
