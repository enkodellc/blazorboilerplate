using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using Microsoft.EntityFrameworkCore;

namespace BlazorBoilerplate.Storage
{
    public class ApiContentDbContext : DbContext
    {
        public ApiContentDbContext(DbContextOptions<ApiContentDbContext> options) : base(options)
        { }

        public DbSet<WikiPage> WikiPages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<WikiPage>().HasIndex(c => new { c.pageid }).IsUnique();
        }
    }
}
