using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.MatchingRules;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class MatchingRuleConfiguration : IEntityTypeConfiguration<MatchingRule>
{
    public void Configure(EntityTypeBuilder<MatchingRule> builder)
    {
        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.Priority)
            .IsRequired();

        builder.Property(mr => mr.RuleDefinition)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(mr => mr.TargetWorkoutPlanId)
            .IsRequired();

        builder.Property(mr => mr.IsActive)
            .HasDefaultValue(true);

        // Foreign key
        builder.HasOne<Domain.WorkoutPlans.WorkoutPlan>()
            .WithMany()
            .HasForeignKey(mr => mr.TargetWorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(mr => mr.Priority);
        builder.HasIndex(mr => mr.IsActive);
    }
}
