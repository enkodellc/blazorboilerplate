using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlazorBoilerplate.Storage
{
    public class TenantStoreDbContext : EFCoreStoreDbContext
    {
        public IConfiguration Configuration { get; }
        private static readonly string projectName = nameof(BlazorBoilerplate);

        public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantInfo>()
                .Property(t => t.ConnectionString)
                .IsRequired(false);
            modelBuilder.Entity<TenantInfo>()
                .HasData(new TenantInfo(projectName, projectName, projectName, null, null));
        }
    }
}