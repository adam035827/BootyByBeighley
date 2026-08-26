using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.Questionnaires;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class QuestionnaireConfiguration : IEntityTypeConfiguration<Questionnaire>
{
    public void Configure(EntityTypeBuilder<Questionnaire> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Question)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(q => q.QuestionNumber)
            .IsRequired();

        builder.Property(q => q.AnswerOptions)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(q => q.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(q => q.IsActive);
        builder.HasIndex(q => q.QuestionNumber);
    }
}
