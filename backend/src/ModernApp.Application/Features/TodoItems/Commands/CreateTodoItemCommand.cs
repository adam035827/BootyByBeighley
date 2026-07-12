using System.ComponentModel.DataAnnotations;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.TodoItems;

namespace ModernApp.Application.Features.TodoItems.Commands;

public record CreateTodoItemCommand(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    string? Description
) : ICommand<TodoItemDto>;

public sealed class CreateTodoItemCommandHandler(ITodoItemRepository repository)
    : ICommandHandler<CreateTodoItemCommand, TodoItemDto>
{
    public async Task<TodoItemDto> ExecuteAsync(CreateTodoItemCommand command, CancellationToken ct)
    {
        var item = TodoItem.Create(command.Title, command.Description);
        await repository.AddAsync(item, ct);
        return item.ToDto();
    }
}
