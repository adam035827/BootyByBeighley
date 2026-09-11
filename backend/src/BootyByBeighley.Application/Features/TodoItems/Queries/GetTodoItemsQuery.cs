using BootyByBeighley.Application.Common.Interfaces;

namespace BootyByBeighley.Application.Features.TodoItems.Queries;

public record GetTodoItemsQuery : IQuery<IReadOnlyList<TodoItemDto>>;

public sealed class GetTodoItemsQueryHandler(ITodoItemRepository repository)
    : IQueryHandler<GetTodoItemsQuery, IReadOnlyList<TodoItemDto>>
{
    public async Task<IReadOnlyList<TodoItemDto>> ExecuteAsync(GetTodoItemsQuery query, CancellationToken ct)
    {
        var items = await repository.GetAllAsync(ct);
        return items.Select(i => i.ToDto()).ToList();
    }
}
