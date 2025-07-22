using DataLibrary.Classes;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Contexts
{
    /// <summary>
    /// Represents the Entity Framework Core database context for user and role management, including accounts, roles, and endpoint permissions.
    /// </summary>
    /// <remarks>
    /// The <see cref="UserDbContext"/> class extends <see cref="IdentityDbContext{Account, AccountRole, String}"/> to manage identity-related entities and their relationships.
    /// It configures entity mappings for accounts, roles, endpoint permissions, and permission actions.
    /// </remarks>
    public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<Account, AccountRole, string>(options)
    {
        internal DbSet<Account> Accounts { get; set; }
        internal DbSet<AccountRole> AccountRoles { get; set; }
        internal DbSet<EndpointPermission> EndpointPermissions { get; set; }
        internal DbSet<EndpointPermissionAction> EndpointPermissionsActions { get; set; }

        /// <summary>
        /// Configures the entity mappings and relationships for identity and authorization entities.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        /// <remarks>
        /// - Configures one-to-many relationships between roles and endpoint permissions, and between endpoint permissions and their actions.
        /// - Calls the base implementation to ensure Identity mappings are applied.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountRole>()
                .HasMany(r => r.EndpointPermissions);

            modelBuilder.Entity<EndpointPermission>()
                .HasMany(r => r.EndpointPermissionActions);

            modelBuilder.Entity<EndpointPermissionAction>()
                .Property(h => h.EndpointPermissionActionType)
                .HasConversion<string>();

        }
    }
}