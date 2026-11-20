using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SkillSync.Identity.Infrastructure.Outbox;

internal class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessage");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(message => message.Content)
            .IsRequired();

        builder.Property(message => message.OccuredAtUtc)
            .IsRequired();

        builder.Property(message => message.Error)
            .HasMaxLength(4000);

        builder.HasIndex(message => message.ProcessedOnUtc);
    }
}
