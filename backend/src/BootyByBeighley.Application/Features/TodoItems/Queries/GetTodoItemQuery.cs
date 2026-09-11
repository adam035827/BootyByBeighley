using BootyByBeighley.Application.Common.Interfaces;

namespace BootyByBeighley.Application.Features.TodoItems.Queries;

public record GetTodoItemQuery(string Id) : IQuery<TodoItemDto?>;

public sealed class GetTodoItemQueryHandler(ITodoItemRepository repository)
    : IQueryHandler<GetTodoItemQuery, TodoItemDto?>
{
    public async Task<TodoItemDto?> ExecuteAsync(GetTodoItemQuery query, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(query.Id, ct);
        return item?.ToDto();
    }
}
