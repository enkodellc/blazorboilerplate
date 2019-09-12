using BlazorBoilerplate.Server.Data.Configurations;
using BlazorBoilerplate.Server.Data.Interfaces;
using BlazorBoilerplate.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Data
{
    //https://trailheadtechnology.com/entity-framework-core-2-1-automate-all-that-boring-boiler-plate/
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<ApiLogItem> ApiLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public DbSet<Message> Messages { get; set; }

        private IUserSession _userSession { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserSession userSession) : base(options)
        {
            _userSession = userSession;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
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
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            modelBuilder.Entity<Message>().ToTable("Messages");

            modelBuilder.ApplyConfiguration(new MessageConfiguration());

            SetGlobalQueryFilters(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        private void SetGlobalQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var tp in modelBuilder.Model.GetEntityTypes())
            {
                var t = tp.ClrType;

                // set global filters
                if (typeof(ISoftDelete).IsAssignableFrom(t))
                {
                    //TODO future for Tenant
                    //if (typeof(ITenantEntity).IsAssignableFrom(t))
                    //{
                    //    // softdeletable and tenant (note do not filter just ITenant - too much filtering!
                    //    // just top level classes that have ITenantEntity
                    //    var method = SetGlobalQueryForSoftDeleteAndTenantMethodInfo.MakeGenericMethod(t);
                    //    method.Invoke(this, new object[] { modelBuilder });
                    //}
                    //else
                    //{
                        // softdeletable
                        var method = SetGlobalQueryForSoftDeleteMethodInfo.MakeGenericMethod(t);
                        method.Invoke(this, new object[] { modelBuilder });
                    //}
                }
            }
        }

        private static readonly MethodInfo SetGlobalQueryForSoftDeleteMethodInfo = typeof(ApplicationDbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForSoftDelete");

        private static readonly MethodInfo SetGlobalQueryForSoftDeleteAndTenantMethodInfo = typeof(ApplicationDbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForSoftDeleteAndTenant");

        public void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            builder.Entity<T>().HasQueryFilter(item => !EF.Property<bool>(item, "IsDeleted"));
        }

        public void SetGlobalQueryForSoftDeleteAndTenant<T>(ModelBuilder builder) where T : class, ISoftDelete, ITenant
        {
            builder.Entity<T>().HasQueryFilter(
                item => !EF.Property<bool>(item, "IsDeleted") &&
                        (_userSession.DisableTenantFilter || EF.Property<int>(item, "TenantId") == _userSession.TenantId));
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

