using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BootyByBeighley.Domain.WorkoutLogs;

namespace BootyByBeighley.Infrastructure.Persistence.Configurations;

internal sealed class WorkoutLogSetEntryConfiguration : IEntityTypeConfiguration<WorkoutLogSetEntry>
{
    public void Configure(EntityTypeBuilder<WorkoutLogSetEntry> builder)
    {
        builder.HasKey(wlse => wlse.Id);

        builder.Property(wlse => wlse.WorkoutLogEntryId)
            .IsRequired();

        builder.Property(wlse => wlse.WorkoutMovementId)
            .IsRequired();

        builder.Property(wlse => wlse.SetNumber)
            .IsRequired();

        builder.Property(wlse => wlse.RepsCompleted)
            .IsRequired();

        builder.Property(wlse => wlse.WeightUsed)
            .HasPrecision(10, 2);

        builder.Property(wlse => wlse.NotesPerMovement)
            .HasMaxLength(500);

        // Foreign keys
        builder.HasOne<Domain.WorkoutPlans.WorkoutMovement>()
            .WithMany()
            .HasForeignKey(wlse => wlse.WorkoutMovementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Note: WorkoutLogEntry relationship is configured in WorkoutLogEntryConfiguration

        // Indexes
        builder.HasIndex(wlse => wlse.WorkoutLogEntryId);
        builder.HasIndex(wlse => wlse.WorkoutMovementId);
        builder.HasIndex(wlse => new { wlse.WorkoutLogEntryId, wlse.SetNumber });
    }
}
