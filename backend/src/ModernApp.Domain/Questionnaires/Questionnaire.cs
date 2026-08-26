using ModernApp.Domain.Common;

namespace ModernApp.Domain.Questionnaires;

public class Questionnaire : Entity
{
    public string Question { get; private set; } = string.Empty;
    public int QuestionNumber { get; private set; }
    public string AnswerOptions { get; private set; } = string.Empty; // JSON array
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Questionnaire() { }

    public static Questionnaire Create(
        string question,
        int questionNumber,
        string answerOptions) // JSON array
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question is required", nameof(question));
        if (questionNumber < 1)
            throw new ArgumentException("QuestionNumber must be positive", nameof(questionNumber));
        if (string.IsNullOrWhiteSpace(answerOptions))
            throw new ArgumentException("AnswerOptions is required", nameof(answerOptions));

        return new Questionnaire
        {
            Question = question,
            QuestionNumber = questionNumber,
            AnswerOptions = answerOptions,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Questionnaire Reconstitute(
        Guid id,
        string question,
        int questionNumber,
        string answerOptions,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new Questionnaire
        {
            Id = id,
            Question = question,
            QuestionNumber = questionNumber,
            AnswerOptions = answerOptions,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
