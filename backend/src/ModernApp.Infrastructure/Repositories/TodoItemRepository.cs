using Microsoft.EntityFrameworkCore;
using ModernApp.Application.Features.TodoItems;
using ModernApp.Domain.TodoItems;
using ModernApp.Infrastructure.Persistence;

namespace ModernApp.Infrastructure.Repositories;

internal sealed class TodoItemRepository(AppDbContext context) : ITodoItemRepository
{
    public async Task<TodoItem?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await context.TodoItems
            .Where(t => t.Id.ToString() == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.TodoItems
            .ToListAsync(ct);
    }

    public async Task AddAsync(TodoItem item, CancellationToken ct = default)
    {
        context.TodoItems.Add(item);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TodoItem item, CancellationToken ct = default)
    {
        context.TodoItems.Update(item);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, ct);
        if (item != null)
        {
            context.TodoItems.Remove(item);
            await context.SaveChangesAsync(ct);
        }
    }
}
