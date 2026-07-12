using ModernApp.Application.Common.Interfaces;

namespace ModernApp.Application.Features.TodoItems.Commands;

public record DeleteTodoItemCommand(string Id) : ICommand<bool>;

public sealed class DeleteTodoItemCommandHandler(ITodoItemRepository repository)
    : ICommandHandler<DeleteTodoItemCommand, bool>
{
    public async Task<bool> ExecuteAsync(DeleteTodoItemCommand command, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(command.Id, ct);
        if (item is null) return false;

        await repository.DeleteAsync(command.Id, ct);
        return true;
    }
}
