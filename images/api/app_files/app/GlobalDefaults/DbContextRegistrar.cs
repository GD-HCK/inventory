using DataLibrary.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Inventory.GlobalDefaults
{
    /// <summary>
    /// Provides methods for registering application database contexts with the dependency injection container.
    /// </summary>
    internal static class DbContextRegistrar
    {
        /// <summary>
        /// Registers the <see cref="AppDbContext"/> and <see cref="UserDbContext"/> with the service collection,
        /// using the SQL Server connection string from the application configuration.
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder"/> used to configure services and access configuration settings.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the required 'MSSQL' connection string is not found in the configuration.
        /// </exception>
        internal static void RegisterDbContexts(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("MSSQL")
                ?? throw new InvalidOperationException("Connection string 'MSSQL' not found.");

            builder.Services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDbContextPool<UserDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
    }
}
