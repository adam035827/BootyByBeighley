using ModernApp.Application.Features.TodoItems;
using ModernApp.Application.Features.TodoItems.Commands;
using ModernApp.Domain.TodoItems;
using NSubstitute;

namespace ModernApp.Application.Tests.Features.TodoItems.Commands;

public sealed class UpdateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository;
    private readonly UpdateTodoItemCommandHandler _sut;

    public UpdateTodoItemCommandHandlerTests()
    {
        _repository = Substitute.For<ITodoItemRepository>();
        _sut = new UpdateTodoItemCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingItem_UpdatesAndReturnsDto()
    {
        var existing = TodoItem.Create("Old title");
        _repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TodoItem?>(existing));

        var command = new UpdateTodoItemCommand(existing.Id, "New title", "New desc", false);
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New title", result!.Title);
        Assert.Equal("New desc", result.Description);
        Assert.False(result.IsCompleted);
        await _repository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IsCompletedTrue_MarksItemCompleted()
    {
        var existing = TodoItem.Create("Task");
        _repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TodoItem?>(existing));

        var command = new UpdateTodoItemCommand(existing.Id, "Task", null, IsCompleted: true);
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        Assert.True(result!.IsCompleted);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task Handle_NonExistentItem_ReturnsNullWithoutUpdate()
    {
        _repository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TodoItem?>(null));

        var command = new UpdateTodoItemCommand("missing-id", "Title", null, false);
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        Assert.Null(result);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<TodoItem>(), Arg.Any<CancellationToken>());
    }
}
