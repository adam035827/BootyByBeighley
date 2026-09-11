using BootyByBeighley.Domain.TodoItems;

namespace BootyByBeighley.Application.Features.TodoItems;

internal static class TodoItemMappings
{
    internal static TodoItemDto ToDto(this TodoItem item) => new(
        item.Id,
        item.Title,
        item.Description,
        item.IsCompleted,
        item.CreatedAt,
        item.CompletedAt);
}
