using ModernApp.Domain.Common;

namespace ModernApp.Domain.TodoItems;

public class TodoItem : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private TodoItem() { }

    public static TodoItem Create(string title, string? description = null) => new()
    {
        Title = title,
        Description = description,
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Reconstitutes a TodoItem from persisted data. Use in repository read paths only.
    /// </summary>
    public static TodoItem Reconstitute(
        string id,
        string title,
        string? description,
        bool isCompleted,
        DateTime createdAt,
        DateTime? completedAt)
    {
        var item = new TodoItem();
        item.Id = id;
        item.Title = title;
        item.Description = description;
        item.IsCompleted = isCompleted;
        item.CreatedAt = createdAt;
        item.CompletedAt = completedAt;
        return item;
    }

    public void Update(string title, string? description)
    {
        Title = title;
        Description = description;
    }

    public void Complete()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void Reopen()
    {
        IsCompleted = false;
        CompletedAt = null;
    }
}
