using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorBoilerplate.Storage.Configurations
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
