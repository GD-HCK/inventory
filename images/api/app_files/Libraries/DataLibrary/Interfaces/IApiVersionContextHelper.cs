using DataLibrary.Classes;

namespace DataLibrary.Interfaces
{
    /// <summary>
    /// Defines methods for retrieving API version information from the data context.
    /// </summary>
    /// <remarks>
    /// The <see cref="IApiVersionContextHelper"/> interface provides a contract for accessing version metadata, such as the latest API version, for use in service discovery or diagnostics.
    /// </remarks>
    public interface IApiVersionContextHelper
    {
        /// <summary>
        /// Gets the latest <see cref="DTOApiVersion"/> for the services.
        /// </summary>
        /// <returns>A <see cref="DTOApiVersion"/> object representing the latest API version information.</returns>
        public DTOApiVersion GetLatestVersion();
    }
}
