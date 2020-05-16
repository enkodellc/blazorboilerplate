using BlazorBoilerplate.Infrastructure.Storage;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Storage.Configurations;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ApiLogItem = BlazorBoilerplate.Shared.DataModels.ApiLogItem;
using Message = BlazorBoilerplate.Shared.DataModels.Message;
using UserProfile = BlazorBoilerplate.Shared.DataModels.UserProfile;

namespace BlazorBoilerplate.Storage
{
    //https://trailheadtechnology.com/entity-framework-core-2-1-automate-all-that-boring-boiler-plate/
    public class ApplicationDbContext : MultiTenantIdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext, IMultiTenantDbContext
    {
        /* We define a default value for TenantInfo. This is a hack. FinBuckle does not provide any method to init TenantInfo or define a default value when seeding the database (in DatabaseInitializer, HttpContext is not yet initialized). */
        public new TenantInfo TenantInfo { get; } = TenantStoreDbContext.DefaultTenant;

        public DbSet<ApiLogItem> ApiLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public DbSet<Message> Messages { get; set; }
        private IUserSession _userSession { get; set; }
        public DbSet<DbLog> Logs { get; set; }

        public ApplicationDbContext(TenantInfo tenantInfo, DbContextOptions<ApplicationDbContext> options, IUserSession userSession)
            : base(tenantInfo, options)
        {
            _userSession = userSession;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Fluent API Does not follow foreign key naming convention
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(a => a.Profile)
                .WithOne(b => b.ApplicationUser)
                .HasForeignKey<UserProfile>(b => b.UserId);

            modelBuilder.ShadowProperties();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole<Guid>>().IsMultiTenant();

            modelBuilder.Entity<Message>().ToTable("Messages");

            modelBuilder.ApplyConfiguration(new MessageConfiguration());

            SetGlobalQueryFilters(modelBuilder);
        }

        private void SetGlobalQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType tp in modelBuilder.Model.GetEntityTypes())
            {
                Type t = tp.ClrType;

                // set Soft Delete Property
                if (typeof(ISoftDelete).IsAssignableFrom(t))
                {
                    MethodInfo method = SetGlobalQueryForSoftDeleteMethodInfo.MakeGenericMethod(t);
                    method.Invoke(this, new object[] { modelBuilder });
                }
            }
        }

        private static readonly MethodInfo SetGlobalQueryForSoftDeleteMethodInfo = typeof(ApplicationDbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForSoftDelete");

        public void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            builder.Entity<T>().HasQueryFilter(item => !EF.Property<bool>(item, "IsDeleted"));
        }

        public override int SaveChanges()
        {
            ChangeTracker.SetShadowProperties(_userSession);
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ChangeTracker.SetShadowProperties(_userSession);
            return await base.SaveChangesAsync(true, cancellationToken);
        }
    }
}