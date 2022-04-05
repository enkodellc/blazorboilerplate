using BlazorBoilerplate.Constants;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace BlazorBoilerplate.Storage
{
    public class TenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
    {
        public static readonly TenantInfo DefaultTenant = new TenantInfo() { Id = Settings.DefaultTenantId, Identifier = Settings.DefaultTenantId, Name = Settings.DefaultTenantId };

        public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantInfo>()
                .Property(t => t.ConnectionString)
                .IsRequired(false);
            modelBuilder.Entity<TenantInfo>()
                .HasData(DefaultTenant);
        }
    }
}