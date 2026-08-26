using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.PersonalRecords;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class PersonalRecordConfiguration : IEntityTypeConfiguration<PersonalRecord>
{
    public void Configure(EntityTypeBuilder<PersonalRecord> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.UserId)
            .IsRequired();

        builder.Property(pr => pr.MovementId)
            .IsRequired();

        builder.Property(pr => pr.MaxWeightKg)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(pr => pr.LoggedAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.Movements.Movement>()
            .WithMany()
            .HasForeignKey(pr => pr.MovementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.WorkoutLogs.WorkoutLogSetEntry>()
            .WithMany()
            .HasForeignKey(pr => pr.WorkoutLogSetEntryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(pr => pr.UserId);
        builder.HasIndex(pr => pr.MovementId);
        builder.HasIndex(pr => pr.LoggedAt);
        builder.HasIndex(pr => new { pr.UserId, pr.MovementId })
            .IsUnique();
    }
}
