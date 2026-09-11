using System.ComponentModel.DataAnnotations;
using BootyByBeighley.Application.Common.Interfaces;

namespace BootyByBeighley.Application.Features.TodoItems.Commands;

/// <summary>
/// HTTP request body for updates. The id comes from the URL route parameter.
/// </summary>
public record UpdateTodoItemRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    string? Description,
    bool IsCompleted);

public record UpdateTodoItemCommand(
    string Id,
    string Title,
    string? Description,
    bool IsCompleted
) : ICommand<TodoItemDto?>;

public sealed class UpdateTodoItemCommandHandler(ITodoItemRepository repository)
    : ICommandHandler<UpdateTodoItemCommand, TodoItemDto?>
{
    public async Task<TodoItemDto?> ExecuteAsync(UpdateTodoItemCommand command, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(command.Id, ct);
        if (item is null) return null;

        item.Update(command.Title, command.Description);

        if (command.IsCompleted) item.Complete();
        else item.Reopen();

        await repository.UpdateAsync(item, ct);
        return item.ToDto();
    }
}
