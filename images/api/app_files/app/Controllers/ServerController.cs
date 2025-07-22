using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Inventory.Controllers
{
    /// <summary>
    /// Provides endpoints for managing server entities, including creation, retrieval, update, deletion, and filtering.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServerController"/> class exposes API endpoints for CRUD operations and filtering of <see cref="Server"/> entities.
    /// It leverages <see cref="IServerContextHelper"/> for data access and supports authorization policies for secure access to server resources.
    /// </remarks>
    [ApiController] // marks the controller behaviour for Api
    [Route("[controller]")] // sets the api  route to match the name of the class without Controller so /weatherforecast
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "EndpointAccessFromRolePermissions")]
    public partial class ServerController(IServerContextHelper serverContextHelper) : ControllerBase
    {
        private readonly IServerContextHelper _serverContextHelper = serverContextHelper;

        /// <summary>
        /// Retrieves a server by its unique identifier.
        /// </summary>
        /// <param name="Id">The unique identifier of the server to retrieve.</param>
        /// <returns>An <see cref="ActionResult{Server}"/> containing the server if found, or an error response.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /servers/1
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///
        /// </remarks>
        /// <response code="200">Returns the requested server.</response>
        /// <response code="400">Invalid server Id supplied.</response>
        /// <response code="404">Server not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Server>> GetByIdAsync(int Id)
        {
            if (Id <= 0)
            {
                return BadRequest($"Invalid server Id {Id}.");
            }

            var server = await _serverContextHelper.GetServerByIdAsync(Id);

            if (server == null)
            {
                return NotFound($"Server with Id {Id} not found.");
            }

            return Ok(server);
        }

        // [GeneratedRegex] gives you better performance, compile-time validation, and cleaner code. It is the modern, recommended approach for defining regular expressions in C# targeting .NET 7 or later.

        [GeneratedRegex(@"([&?])?page=\d+")]
        private static partial Regex RemovePageRegex();

        [GeneratedRegex(@"[&]{2,}")]
        private static partial Regex RemoveQueryStringCharactersRegex();

        private string GetLink()
        {
            var link = Request.Scheme + "://" + Request.Host + Request.Path;

            var queryString = Request.QueryString.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(queryString))
            {
                // Remove all occurrences of page=<number> (with or without a leading & or ?)
                queryString = RemovePageRegex().Replace(queryString, "");

                // Remove any leftover double ampersands or leading/trailing ampersands
                queryString = RemoveQueryStringCharactersRegex().Replace(queryString, "&");

                // Remove trailing or leading '?' or '&' if they exist
                queryString = queryString.TrimEnd('?', '&').TrimStart('?', '&') + '&';
            }

            link = link + "?" + queryString + "page=";

            return link;
        }

        /// <summary>
        /// Retrieves a paginated list of servers filtered by the specified criteria.
        /// </summary>
        /// <param name="filters">A dictionary of filter criteria to apply to the server list (e.g., name, IP address).</param>
        /// <returns>An <see cref="IActionResult"/> containing a paginated list of servers matching the filter.</returns>
        /// <remarks>
        /// Sample requests:
        ///
        ///     GET /servers
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///
        ///     GET /servers?name=svr_1&amp;ipaddress=1.1.1.1
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///
        /// </remarks>
        /// <response code="200">Returns a paginated list of servers.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetByFilter([FromQuery] Dictionary<string, string> filters)
        {
            var result = _serverContextHelper.GetServers(filters);

            var link = GetLink();

            if (!string.IsNullOrEmpty(result.Links.Next))
                result.Links.Next = link + result.Links.Next;

            if (!string.IsNullOrEmpty(result.Links.Previous))
                result.Links.Previous = link + result.Links.Previous;

            return Ok(result);
        }

        /// <summary>
        /// Adds a new server to the inventory.
        /// </summary>
        /// <param name="server">The <see cref="Server"/> object to add.</param>
        /// <returns>An <see cref="ActionResult{Server}"/> containing the created server.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /servers
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///     Body:
        ///     {
        ///       "Name": "Server_1",
        ///       "Scopes": [ { "Scope": "Web" }, { "Scope": "Dns" } ],
        ///       "IPAddress": "10.0.0.1",
        ///       "OS": { "OS": "Web" }
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Server created successfully.</response>
        /// <response code="400">Malformed server object.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Server>> PostAsync([FromBody] Server server)
        {
            var newServer = await _serverContextHelper.PostServerAsync(server);

            return CreatedAtAction("GetById", new { newServer.Id }, newServer);
        }

        /// <summary>
        /// Updates one or more properties of an existing server.
        /// </summary>
        /// <param name="Id">The unique identifier of the server to update.</param>
        /// <param name="serverPatch">The <see cref="ServerPatch"/> object containing the properties to update.</param>
        /// <returns>An <see cref="ActionResult{Server}"/> containing the updated server.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /servers/1
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///     Body:
        ///     {
        ///       "Id": 1,
        ///       "Name": "Server_1",
        ///       "Scopes": [ { "Scope": "Web" }, { "Scope": "Dns" } ],
        ///       "IPAddress": "10.0.0.1"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Server updated successfully.</response>
        /// <response code="400">Invalid or mismatched server Id, or unrecognized property.</response>
        /// <response code="404">Server not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPatch("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Server>> PatchAsync([FromRoute] int Id, [FromBody] ServerPatch serverPatch)
        {
            if (Id <= 0)
                return BadRequest($"Invalid server Id {Id}.");

            if (Id != serverPatch.Id)
                return BadRequest($"Body and route Ids do not match. Check you're updating the correct server");

            return Ok(await _serverContextHelper.PatchServerAsync(serverPatch));
        }

        /// <summary>
        /// Updates an existing server record in the inventory.
        /// </summary>
        /// <param name="Id">The unique identifier of the server to update.</param>
        /// <param name="server">The <see cref="Server"/> object with updated properties.</param>
        /// <returns>An <see cref="ActionResult{Server}"/> containing the updated server.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /servers/1
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///     Body:
        ///     {
        ///       "Id": 1,
        ///       "Name": "Server_1",
        ///       "Scopes": [ { "Scope": "Web" }, { "Scope": "Dns" } ],
        ///       "IPAddress": "10.0.0.1",
        ///       "OS": { "OS": "Web" }
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Server updated successfully.</response>
        /// <response code="400">Malformed server object or mismatched Id.</response>
        /// <response code="404">Server not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Server>> PutAsync([FromRoute] int Id, [FromBody] Server server)
        {
            if (Id <= 0)
                return BadRequest($"Invalid server Id {Id}.");

            if (Id != server.Id)
                return BadRequest($"Body and route Ids do not match. Check you're updating the correct server");

            var newServer = await _serverContextHelper.PutServerAsync(server);

            return Ok(newServer);
        }

        /// <summary>
        /// Deletes a server record from the inventory.
        /// </summary>
        /// <param name="Id">The unique identifier of the server to delete.</param>
        /// <returns>An <see cref="ActionResult"/> indicating the result of the operation.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /servers/1
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///
        /// </remarks>
        /// <response code="204">Server deleted successfully.</response>
        /// <response code="400">Invalid server Id supplied.</response>
        /// <response code="404">Server not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{Id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAsync(int Id)
        {
            if (Id <= 0)
                return BadRequest($"Invalid server Id {Id}.");

            await _serverContextHelper.DeleteServerAsync(Id);

            return Ok(Id);
        }

        /// <summary>
        /// Returns the allowed HTTP methods for this controller by setting the Allow header.
        /// </summary>
        /// <returns>HTTP 200 OK with the Allow header and body listing supported HTTP methods.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     OPTIONS /servers
        ///     Headers:
        ///         Apikey: your_apikey
        ///     OR
        ///         Authorization: Basic "base64(username:password)"
        ///
        /// </remarks>
        /// <response code="200">Returns a list of available HTTP methods for the /servers endpoint in the Allow header.</response>
        /// <response code="500">Internal server error.</response>
        [HttpOptions]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Options()
        {
            var verbs = new HashSet<string> {
                HttpMethod.Get.Method,
                HttpMethod.Post.Method,
                HttpMethod.Put.Method,
                HttpMethod.Delete.Method,
                HttpMethod.Patch.Method,
                HttpMethod.Options.Method
            };

            Response.Headers.Append("Allow", string.Join(", ", verbs));

            return Ok(verbs);
        }
    }
}
