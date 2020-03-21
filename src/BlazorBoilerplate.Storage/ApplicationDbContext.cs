using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApiLogItem = BlazorBoilerplate.Shared.DataModels.ApiLogItem;
using Message = BlazorBoilerplate.Shared.DataModels.Message;
using UserProfile = BlazorBoilerplate.Shared.DataModels.UserProfile;

namespace BlazorBoilerplate.Storage
{
    //https://trailheadtechnology.com/entity-framework-core-2-1-automate-all-that-boring-boiler-plate/
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
    {
        public DbSet<ApiLogItem> ApiLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Book> Books { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private IUserSession _userSession { get; set; }

        public Guid TenantId
        {
            get
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    System.Security.Claims.Claim tenantClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(predicate: c => c.Type == Core.ClaimConstants.TenantId);
                    if (tenantClaim != null) // user belongs to a tenant
                    {
                        return Guid.Parse(tenantClaim.Value);
                    }
                }
                return (Tenants.FirstOrDefault(t => t.Title == Core.TenantConstants.RootTenantTitle)).Id;
            }
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor, IUserSession userSession) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _userSession = userSession;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Fluent API Does not follow foreign key naming convention
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(a => a.Profile)
                .WithOne(b => b.ApplicationUser)
                .HasForeignKey<UserProfile>(b => b.UserId);

            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.Title)
                .IsUnique(true);

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
                Type t = tp.ClrType;

                // set global filters
                if (typeof(ITenant).IsAssignableFrom(t))
                {
                    MethodInfo method = SetGlobalQueryForTenantMethodInfo.MakeGenericMethod(t);
                    method.Invoke(this, new object[] { modelBuilder });
                }
                if (typeof(ISoftDelete).IsAssignableFrom(t))
                {
                    MethodInfo method = SetGlobalQueryForSoftDeleteMethodInfo.MakeGenericMethod(t);
                    method.Invoke(this, new object[] { modelBuilder });
                }
            }
        }

        private static readonly MethodInfo SetGlobalQueryForSoftDeleteMethodInfo = typeof(ApplicationDbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForSoftDelete");

        private static readonly MethodInfo SetGlobalQueryForTenantMethodInfo = typeof(ApplicationDbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForTenant");

        public void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            builder.Entity<T>().HasQueryFilter(item => !EF.Property<bool>(item, "IsDeleted"));
        }

        public void SetGlobalQueryForTenant<T>(ModelBuilder builder) where T : class, ITenant
        {
            builder.Entity<T>().HasQueryFilter(item =>
            (_userSession.DisableTenantFilter || EF.Property<Guid>(item, "TenantId") == TenantId));
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