using BlazorBoilerplate.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorBoilerplate.Server.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.Property(g => g.UserName)
                .IsRequired();

            builder.Property(g => g.Text)
                .IsRequired();

            builder.HasOne(a => a.Sender)
                .WithMany(m => m.Messages)
                .HasForeignKey(u => u.UserID);
        }
    }
}
