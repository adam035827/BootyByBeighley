using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BootyByBeighley.Domain.PlanEnrollments;

namespace BootyByBeighley.Infrastructure.Persistence.Configurations;

internal sealed class PlanEnrollmentConfiguration : IEntityTypeConfiguration<PlanEnrollment>
{
    public void Configure(EntityTypeBuilder<PlanEnrollment> builder)
    {
        builder.HasKey(pe => pe.Id);

        builder.Property(pe => pe.UserId)
            .IsRequired();

        builder.Property(pe => pe.WorkoutPlanId)
            .IsRequired();

        builder.Property(pe => pe.EnrolledAt)
            .IsRequired();

        builder.Property(pe => pe.StartDate)
            .IsRequired();

        builder.Property(pe => pe.PlannedEndDate)
            .IsRequired();

        builder.Property(pe => pe.SelectedTrainingDays)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(pe => pe.Status)
            .IsRequired()
            .HasConversion<int>();

        // Foreign keys
        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(pe => pe.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.WorkoutPlans.WorkoutPlan>()
            .WithMany()
            .HasForeignKey(pe => pe.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pe => pe.UserId);
        builder.HasIndex(pe => pe.WorkoutPlanId);
        builder.HasIndex(pe => new { pe.UserId, pe.WorkoutPlanId })
            .IsUnique();
        builder.HasIndex(pe => pe.Status);
    }
}
