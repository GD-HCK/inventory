using DataLibrary.Classes;

namespace DataLibrary.Interfaces
{
    /// <summary>
    /// Defines methods for managing server entities, including creation, retrieval, update, deletion, and filtering.
    /// </summary>
    /// <remarks>
    /// The <see cref="IServerContextHelper"/> interface provides a contract for CRUD operations and filtering on <see cref="Server"/> entities,
    /// supporting both synchronous and asynchronous patterns for use in data access layers or service contexts.
    /// </remarks>
    public interface IServerContextHelper
    {
        /// <summary>
        /// Adds a new <see cref="Server"/>.
        /// </summary>
        /// <param name="server">The <see cref="Server"/> object to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="DTOServer"/>.</returns>
        public Task<DTOServer> PostServerAsync(Server server);

        /// <summary>
        /// Updates properties of an existing <see cref="Server"/>.
        /// </summary>
        /// <param name="serverPatch">The <see cref="ServerPatch"/> object containing the properties to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="DTOServer"/>.</returns>
        public Task<DTOServer> PatchServerAsync(ServerPatch serverPatch);

        /// <summary>
        /// Gets a list of servers by filter.
        /// </summary>
        /// <param name="filters">A dictionary of filter criteria to apply to the server list.</param>
        /// <returns>A <see cref="PaginatedList{DTOServer}"/> containing the filtered servers.</returns>
        public PaginatedList<DTOServer> GetServers(Dictionary<string, string> filters);

        /// <summary>
        /// Gets a server by its Id.
        /// </summary>
        /// <param name="Id">The Id of the <see cref="Server"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="DTOServer"/> with the specified Id.</returns>
        public Task<DTOServer> GetServerByIdAsync(int Id);

        /// <summary>
        /// Updates an existing <see cref="Server"/>.
        /// </summary>
        /// <param name="server">The <see cref="Server"/> object to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="DTOServer"/>.</returns>
        public Task<DTOServer> PutServerAsync(Server server);

        /// <summary>
        /// Deletes an existing <see cref="Server"/> by its Id.
        /// </summary>
        /// <param name="Id">The Id of the <see cref="Server"/> to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the Id of the deleted <see cref="Server"/>.</returns>
        public Task<int> DeleteServerAsync(int Id);

    }
}
