using ModernApp.Domain.TodoItems;

namespace ModernApp.Infrastructure.Repositories;

internal static class TodoItemMappings
{
    internal static TodoItemDocument ToDocument(this TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        IsCompleted = item.IsCompleted,
        CreatedAt = item.CreatedAt,
        CompletedAt = item.CompletedAt
    };

    internal static TodoItem ToDomain(this TodoItemDocument doc) =>
        TodoItem.Reconstitute(doc.Id, doc.Title, doc.Description, doc.IsCompleted, doc.CreatedAt, doc.CompletedAt);
}
