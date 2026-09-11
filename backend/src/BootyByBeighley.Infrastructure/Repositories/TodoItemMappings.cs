using BootyByBeighley.Domain.TodoItems;

namespace BootyByBeighley.Infrastructure.Repositories;

internal static class TodoItemMappings
{
    internal static TodoItemDocument ToDocument(this TodoItem item) => new()
    {
        Id = item.Id.ToString(),
        Title = item.Title,
        Description = item.Description,
        IsCompleted = item.IsCompleted,
        CreatedAt = item.CreatedAt,
        CompletedAt = item.CompletedAt
    };

    internal static TodoItem ToDomain(this TodoItemDocument doc) =>
        TodoItem.Reconstitute(Guid.Parse(doc.Id), doc.Title, doc.Description, doc.IsCompleted, doc.CreatedAt, doc.CompletedAt);
}
