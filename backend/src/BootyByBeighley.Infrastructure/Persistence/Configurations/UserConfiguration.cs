using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BootyByBeighley.Domain.Users;

namespace BootyByBeighley.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.SubscriptionStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.ProfilePhotoUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.Bio)
            .HasMaxLength(1000);

        builder.Property(u => u.SocialLinks)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.Role);
    }
}
