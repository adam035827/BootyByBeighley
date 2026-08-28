using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.WorkoutPlans;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.Description)
            .HasMaxLength(2000);

        builder.Property(w => w.WorkoutPlanId)
            .IsRequired();

        builder.Property(w => w.Order)
            .IsRequired();

        // Relationship: Workout -> WorkoutMovements
        builder.HasMany(w => w.Movements)
            .WithOne(wm => wm.Workout)
            .HasForeignKey(wm => wm.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => w.WorkoutPlanId);
        builder.HasIndex(w => new { w.WorkoutPlanId, w.Order })
            .IsUnique();
    }
}
