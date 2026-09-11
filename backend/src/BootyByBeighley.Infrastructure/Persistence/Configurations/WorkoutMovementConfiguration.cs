using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BootyByBeighley.Domain.WorkoutPlans;

namespace BootyByBeighley.Infrastructure.Persistence.Configurations;

internal sealed class WorkoutMovementConfiguration : IEntityTypeConfiguration<WorkoutMovement>
{
    public void Configure(EntityTypeBuilder<WorkoutMovement> builder)
    {
        builder.HasKey(wm => wm.Id);

        builder.Property(wm => wm.WorkoutId)
            .IsRequired();

        builder.Property(wm => wm.MovementId)
            .IsRequired();

        builder.Property(wm => wm.Order)
            .IsRequired();

        builder.Property(wm => wm.PrescribedSets)
            .IsRequired();

        builder.Property(wm => wm.PrescribedReps)
            .IsRequired();

        // Note: Workout relationship is configured in WorkoutConfiguration
        
        // Foreign key to Movement
        builder.HasOne<Domain.Movements.Movement>()
            .WithMany()
            .HasForeignKey(wm => wm.MovementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wm => wm.WorkoutId);
        builder.HasIndex(wm => wm.MovementId);
        builder.HasIndex(wm => new { wm.WorkoutId, wm.Order })
            .IsUnique();
    }
}
