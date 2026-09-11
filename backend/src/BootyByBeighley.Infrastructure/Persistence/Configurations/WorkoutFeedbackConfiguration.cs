using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BootyByBeighley.Domain.WorkoutLogs;

namespace BootyByBeighley.Infrastructure.Persistence.Configurations;

internal sealed class WorkoutFeedbackConfiguration : IEntityTypeConfiguration<WorkoutFeedback>
{
    public void Configure(EntityTypeBuilder<WorkoutFeedback> builder)
    {
        builder.HasKey(wf => wf.Id);

        builder.Property(wf => wf.WorkoutLogEntryId)
            .IsRequired();

        builder.Property(wf => wf.UserId)
            .IsRequired();

        builder.Property(wf => wf.DifficultyRating)
            .IsRequired();

        builder.Property(wf => wf.Comment)
            .HasMaxLength(500);

        builder.Property(wf => wf.SubmittedAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne<WorkoutLogEntry>()
            .WithMany()
            .HasForeignKey(wf => wf.WorkoutLogEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(wf => wf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wf => wf.WorkoutLogEntryId)
            .IsUnique();
        builder.HasIndex(wf => wf.UserId);
        builder.HasIndex(wf => wf.SubmittedAt);
    }
}
