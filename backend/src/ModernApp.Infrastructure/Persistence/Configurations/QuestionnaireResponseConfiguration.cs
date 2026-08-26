using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModernApp.Domain.Questionnaires;

namespace ModernApp.Infrastructure.Persistence.Configurations;

internal sealed class QuestionnaireResponseConfiguration : IEntityTypeConfiguration<QuestionnaireResponse>
{
    public void Configure(EntityTypeBuilder<QuestionnaireResponse> builder)
    {
        builder.HasKey(qr => qr.Id);

        builder.Property(qr => qr.UserId)
            .IsRequired();

        builder.Property(qr => qr.QuestionnaireId)
            .IsRequired();

        builder.Property(qr => qr.SelectedAnswer)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(qr => qr.AnsweredAt)
            .IsRequired();

        // Foreign keys
        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(qr => qr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Questionnaire>()
            .WithMany()
            .HasForeignKey(qr => qr.QuestionnaireId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(qr => qr.UserId);
        builder.HasIndex(qr => qr.QuestionnaireId);
        builder.HasIndex(qr => qr.AnsweredAt);
    }
}
