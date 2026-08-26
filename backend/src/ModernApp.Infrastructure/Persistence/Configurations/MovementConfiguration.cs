using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.Movements;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class MovementConfiguration : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.VideoUrl)
            .HasMaxLength(2048);

        builder.Property(m => m.VideoCaption)
            .HasMaxLength(5000);

        builder.Property(m => m.DefaultSets)
            .IsRequired();

        builder.Property(m => m.DefaultReps)
            .IsRequired();

        // Indexes
        builder.HasIndex(m => m.Name);
        builder.HasIndex(m => m.CreatedAt);
    }
}
