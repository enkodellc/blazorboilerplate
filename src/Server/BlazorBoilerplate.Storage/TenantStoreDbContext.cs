using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlazorBoilerplate.Storage
{
    public class TenantStoreDbContext : EFCoreStoreDbContext
    {
        public IConfiguration Configuration { get; }
        public static string DefaultTenantId { get; private set; }

        public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
            DefaultTenantId = configuration["BlazorBoilerplate:DefaultTenantId"];
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantInfo>()
                .Property(t => t.ConnectionString)
                .IsRequired(false);
            modelBuilder.Entity<TenantInfo>()
                .HasData(new TenantInfo(DefaultTenantId, DefaultTenantId, DefaultTenantId, null, null));
        }
    }
}