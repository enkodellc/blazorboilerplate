using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using Microsoft.EntityFrameworkCore;

namespace BlazorBoilerplate.Storage
{
    public class LocalizationDbContext : DbContext
    {
        public LocalizationDbContext(DbContextOptions<LocalizationDbContext> options) : base(options)
        { }

        public DbSet<LocalizationRecord> LocalizationRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<LocalizationRecord>().HasAlternateKey(c => new { c.Key, c.LocalizationCulture, c.ResourceKey });
        }
    }
}
