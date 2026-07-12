using ModernApp.Domain.TodoItems;

namespace ModernApp.Application.Features.TodoItems;

public interface ITodoItemRepository
{
    Task<TodoItem?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TodoItem item, CancellationToken ct = default);
    Task UpdateAsync(TodoItem item, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
