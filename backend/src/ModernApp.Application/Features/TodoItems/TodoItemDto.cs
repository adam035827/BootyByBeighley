namespace ModernApp.Application.Features.TodoItems;

public record TodoItemDto(
    string Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);
