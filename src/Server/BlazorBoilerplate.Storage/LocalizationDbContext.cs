using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using Microsoft.EntityFrameworkCore;

namespace BlazorBoilerplate.Storage
{
    public class LocalizationDbContext : DbContext
    {
        public LocalizationDbContext(DbContextOptions<LocalizationDbContext> options) : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Suppress the PendingModelChangesWarning to resolve the error
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<PluralFormRule> PluralFormRules { get; set; }

        public DbSet<PluralTranslation> PluralTranslations { get; set; }

        public DbSet<LocalizationRecord> LocalizationRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<LocalizationRecord>().HasIndex(c => new { c.MsgId, c.Culture, c.ContextId }).IsUnique();
        }
    }
}
