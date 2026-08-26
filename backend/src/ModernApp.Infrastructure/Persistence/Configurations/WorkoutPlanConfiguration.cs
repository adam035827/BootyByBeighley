using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.WorkoutPlans;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasKey(wp => wp.Id);

        builder.Property(wp => wp.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(wp => wp.Description)
            .HasMaxLength(2000);

        builder.Property(wp => wp.Difficulty)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(wp => wp.IsPublished)
            .HasDefaultValue(false);

        // Relationship: WorkoutPlan -> next WorkoutPlan (self-referencing)
        builder.HasOne<WorkoutPlan>()
            .WithMany()
            .HasForeignKey(wp => wp.NextPhasePlanId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: WorkoutPlan -> Workouts
        builder.HasMany<Workout>()
            .WithOne()
            .HasForeignKey(w => w.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wp => wp.IsPublished);
        builder.HasIndex(wp => wp.CreatedAt);
    }
}
