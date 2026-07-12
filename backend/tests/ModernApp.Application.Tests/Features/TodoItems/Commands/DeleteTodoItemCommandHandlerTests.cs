using ModernApp.Application.Features.TodoItems;
using ModernApp.Application.Features.TodoItems.Commands;
using ModernApp.Domain.TodoItems;
using NSubstitute;

namespace ModernApp.Application.Tests.Features.TodoItems.Commands;

public sealed class DeleteTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository;
    private readonly DeleteTodoItemCommandHandler _sut;

    public DeleteTodoItemCommandHandlerTests()
    {
        _repository = Substitute.For<ITodoItemRepository>();
        _sut = new DeleteTodoItemCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingItem_DeletesAndReturnsTrue()
    {
        var existing = TodoItem.Create("Task to delete");
        _repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TodoItem?>(existing));

        var result = await _sut.ExecuteAsync(new DeleteTodoItemCommand(existing.Id), CancellationToken.None);

        Assert.True(result);
        await _repository.Received(1).DeleteAsync(existing.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentItem_ReturnsFalseWithoutDeletion()
    {
        _repository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TodoItem?>(null));

        var result = await _sut.ExecuteAsync(new DeleteTodoItemCommand("ghost-id"), CancellationToken.None);

        Assert.False(result);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
