using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BootyByBeighley.Domain.WorkoutLogs;

namespace BootyByBeighley.Infrastructure.Persistence.Configurations;

internal sealed class MissedWorkoutConfiguration : IEntityTypeConfiguration<MissedWorkout>
{
    public void Configure(EntityTypeBuilder<MissedWorkout> builder)
    {
        builder.HasKey(mw => mw.Id);

        builder.Property(mw => mw.WorkoutLogEntryId)
            .IsRequired();

        builder.Property(mw => mw.UserId)
            .IsRequired();

        builder.Property(mw => mw.WorkoutId)
            .IsRequired();

        builder.Property(mw => mw.MissedDate)
            .IsRequired();

        builder.Property(mw => mw.MissedReason)
            .HasMaxLength(500);

        builder.Property(mw => mw.Notes)
            .HasMaxLength(1000);

        // Foreign keys
        builder.HasOne<WorkoutLogEntry>()
            .WithMany()
            .HasForeignKey(mw => mw.WorkoutLogEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(mw => mw.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.WorkoutPlans.Workout>()
            .WithMany()
            .HasForeignKey(mw => mw.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(mw => mw.UserId);
        builder.HasIndex(mw => mw.WorkoutId);
        builder.HasIndex(mw => mw.MissedDate);
    }
}
