using ModernApp.Domain.Common;

namespace ModernApp.Domain.Movements;

public class Movement : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DefaultSets { get; private set; }
    public int DefaultReps { get; private set; }
    public int? DefaultRestSeconds { get; private set; }
    public string? VideoUrl { get; private set; }
    public string? VideoCaption { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Movement() { }

    public static Movement Create(
        string name,
        int defaultSets,
        int defaultReps,
        string? description = null,
        int? defaultRestSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (defaultSets < 1 || defaultSets > 20)
            throw new ArgumentException("DefaultSets must be between 1 and 20", nameof(defaultSets));
        if (defaultReps < 1 || defaultReps > 100)
            throw new ArgumentException("DefaultReps must be between 1 and 100", nameof(defaultReps));
        if (defaultRestSeconds.HasValue && (defaultRestSeconds < 0 || defaultRestSeconds > 300))
            throw new ArgumentException("DefaultRestSeconds must be between 0 and 300", nameof(defaultRestSeconds));

        return new Movement
        {
            Name = name,
            DefaultSets = defaultSets,
            DefaultReps = defaultReps,
            Description = description,
            DefaultRestSeconds = defaultRestSeconds,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Movement Reconstitute(
        Guid id,
        string name,
        string? description,
        int defaultSets,
        int defaultReps,
        int? defaultRestSeconds,
        string? videoUrl,
        string? videoCaption,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new Movement
        {
            Id = id,
            Name = name,
            Description = description,
            DefaultSets = defaultSets,
            DefaultReps = defaultReps,
            DefaultRestSeconds = defaultRestSeconds,
            VideoUrl = videoUrl,
            VideoCaption = videoCaption,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void UpdateVideo(string videoUrl, string videoCaption)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new ArgumentException("VideoUrl is required", nameof(videoUrl));
        if (string.IsNullOrWhiteSpace(videoCaption))
            throw new ArgumentException("VideoCaption is required", nameof(videoCaption));
        if (videoCaption.Length > 5000)
            throw new ArgumentException("VideoCaption must not exceed 5000 characters", nameof(videoCaption));

        VideoUrl = videoUrl;
        VideoCaption = videoCaption;
        UpdatedAt = DateTime.UtcNow;
    }
}
