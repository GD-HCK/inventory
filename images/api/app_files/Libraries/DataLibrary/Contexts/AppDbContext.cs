using DataLibrary.Classes;
using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Contexts
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the application's core data, including servers, health checks, and API versioning.
    /// </summary>
    /// <remarks>
    /// The <see cref="AppDbContext"/> class manages the application's main data entities and their relationships. It configures entity mappings, relationships, and value conversions for enums and complex types.
    /// </remarks>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        internal DbSet<HealthCheckResult> HealthCheckResults { get; set; }
        internal DbSet<Server> Servers { get; set; }
        internal DbSet<ServerScope> ServerScopes { get; set; }
        internal DbSet<ServerOS> ServersOS { get; set; }
        internal DbSet<ApiVersion> ApiVersions { get; set; }

        /// <summary>
        /// Configures the entity mappings, relationships, and value conversions for the model.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        /// <remarks>
        /// - Configures one-to-many and one-to-one relationships between servers, scopes, and operating systems.
        /// - Sets up unique indexes on server name and IP address.
        /// - Converts enum properties to strings for database storage.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerScope>()
                .HasOne(s => s.Server)
                .WithMany(s => s.Scopes);

            modelBuilder.Entity<ServerScope>()
                .Property(h => h.ScopeType)
                .HasConversion<string>();

            modelBuilder.Entity<ServerOS>()
                .Property(h => h.ServerOSType)
                .HasConversion<string>();

            modelBuilder.Entity<HealthCheckResult>()
                .Property(h => h.Status)
                .HasConversion<string>();

            modelBuilder.Entity<HealthCheckResult>()
                .Property(h => h.Service)
                .HasConversion<string>();

            modelBuilder.Entity<Server>()
                .HasOne(s => s.OS)
                .WithOne(o => o.Server)
                .HasForeignKey<ServerOS>(o => o.ServerId)
                .IsRequired();

            modelBuilder.Entity<Server>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Server>()
                .HasIndex(s => s.IPAddress)
                .IsUnique();
        }
    }
}