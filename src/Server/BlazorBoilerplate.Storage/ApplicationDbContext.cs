using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Interfaces;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using ApiLogItem = BlazorBoilerplate.Infrastructure.Storage.DataModels.ApiLogItem;
using UserProfile = BlazorBoilerplate.Infrastructure.Storage.DataModels.UserProfile;

namespace BlazorBoilerplate.Storage
{
    public class ApplicationDbContext : MultiTenantIdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IMultiTenantDbContext
    {
        public DbSet<TenantSetting> TenantSettings { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ApiLogItem> ApiLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<QueuedEmail> QueuedEmails { get; set; }
        private IUserSession UserSession { get; set; }
        public DbSet<DbLog> Logs { get; set; }

        /* We define a default value for TenantInfo. This is a hack. FinBuckle does not provide any method to init TenantInfo or define a default value when seeding the database (in DatabaseInitializer, HttpContext is not yet initialized). */
        public ApplicationDbContext(TenantInfo tenantInfo, DbContextOptions<ApplicationDbContext> options, IUserSession userSession)
            : base(tenantInfo ?? TenantStoreDbContext.DefaultTenant, options)
        {
            TenantNotSetMode = TenantNotSetMode.Overwrite;
            TenantMismatchMode = TenantMismatchMode.Overwrite;
            UserSession = userSession;            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.HasOne(a => a.Profile)
                .WithOne(b => b.ApplicationUser)
                .HasForeignKey<UserProfile>(b => b.UserId);

                b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(e => e.User)
                .WithOne(d => d.Person)
                .HasForeignKey<ApplicationUser>(d => d.PersonId);

                entity.HasOne(d => d.CreatedBy)
                   .WithMany()
                   .HasForeignKey(p => p.CreatedById)
                   .OnDelete(DeleteBehavior.ClientCascade);

                entity.HasOne(d => d.ModifiedBy)
                   .WithMany()
                   .HasForeignKey(p => p.ModifiedById)
                   .OnDelete(DeleteBehavior.ClientCascade);
            });

            modelBuilder.Entity<Company>().HasIndex(e => e.VatIn).IsUnique();

            modelBuilder.Entity<ApiLogItem>(b =>
            {
                b.HasOne(e => e.ApplicationUser)
                    .WithMany(e => e.ApiLogItems)
                    .HasForeignKey(e => e.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QueuedEmail>(b =>
            {
                b.Property(b => b.CreatedOn).HasDefaultValueSql("getdate()");
            });

            modelBuilder.ShadowProperties();

            modelBuilder.Entity<TenantSetting>().ToTable("TenantSettings").HasKey(i => new { i.TenantId, i.Key });

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
            ChangeTracker.SetShadowProperties(UserSession);
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.SetShadowProperties(UserSession);
            return await base.SaveChangesAsync(true, cancellationToken);
        }
    }
}