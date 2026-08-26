namespace ModernApp.Application.Features.TodoItems;

public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);
