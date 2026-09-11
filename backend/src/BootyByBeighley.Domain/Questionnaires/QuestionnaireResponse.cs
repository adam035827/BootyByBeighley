using BootyByBeighley.Domain.Common;

namespace BootyByBeighley.Domain.Questionnaires;

public class QuestionnaireResponse : Entity
{
    public Guid UserId { get; private set; }
    public Guid QuestionnaireId { get; private set; }
    public string SelectedAnswer { get; private set; } = string.Empty;
    public DateTime AnsweredAt { get; private set; }

    private QuestionnaireResponse() { }

    public static QuestionnaireResponse Create(
        Guid userId,
        Guid questionnaireId,
        string selectedAnswer)
    {
        if (string.IsNullOrWhiteSpace(selectedAnswer))
            throw new ArgumentException("SelectedAnswer is required", nameof(selectedAnswer));

        return new QuestionnaireResponse
        {
            UserId = userId,
            QuestionnaireId = questionnaireId,
            SelectedAnswer = selectedAnswer,
            AnsweredAt = DateTime.UtcNow
        };
    }

    public static QuestionnaireResponse Reconstitute(
        Guid id,
        Guid userId,
        Guid questionnaireId,
        string selectedAnswer,
        DateTime answeredAt)
    {
        return new QuestionnaireResponse
        {
            Id = id,
            UserId = userId,
            QuestionnaireId = questionnaireId,
            SelectedAnswer = selectedAnswer,
            AnsweredAt = answeredAt
        };
    }
}
