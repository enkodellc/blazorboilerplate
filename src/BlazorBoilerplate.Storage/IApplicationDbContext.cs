using System;
using System.Threading;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using Microsoft.EntityFrameworkCore;

namespace BlazorBoilerplate.Storage
{
    public interface IApplicationDbContext
    {
        public DbSet<ApiLogItem> ApiLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public Guid TenantId { get; }

        public void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : class, ISoftDelete;

        public void SetGlobalQueryForTenant<T>(ModelBuilder builder) where T : class, ITenant;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        int SaveChanges();
    }
}
