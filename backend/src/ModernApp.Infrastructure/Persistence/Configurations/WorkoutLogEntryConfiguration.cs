using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.PlanEnrollments;
using ModernApp.Domain.WorkoutLogs;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class WorkoutLogEntryConfiguration : IEntityTypeConfiguration<WorkoutLogEntry>
{
    public void Configure(EntityTypeBuilder<WorkoutLogEntry> builder)
    {
        builder.HasKey(wle => wle.Id);

        builder.Property(wle => wle.UserId)
            .IsRequired();

        builder.Property(wle => wle.WorkoutId)
            .IsRequired();

        builder.Property(wle => wle.PlanEnrollmentId)
            .IsRequired();

        builder.Property(wle => wle.CompletedAt)
            .IsRequired();

        builder.Property(wle => wle.Notes)
            .HasMaxLength(1000);

        builder.Property(wle => wle.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(wle => wle.MissedReason)
            .HasMaxLength(500);

        // Foreign keys
        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(wle => wle.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.WorkoutPlans.Workout>()
            .WithMany()
            .HasForeignKey(wle => wle.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlanEnrollment>()
            .WithMany()
            .HasForeignKey(wle => wle.PlanEnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation property
        builder.HasMany<WorkoutLogSetEntry>()
            .WithOne()
            .HasForeignKey(wlse => wlse.WorkoutLogEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wle => wle.UserId);
        builder.HasIndex(wle => wle.WorkoutId);
        builder.HasIndex(wle => wle.CompletedAt);
        builder.HasIndex(wle => wle.Status);
    }
}
