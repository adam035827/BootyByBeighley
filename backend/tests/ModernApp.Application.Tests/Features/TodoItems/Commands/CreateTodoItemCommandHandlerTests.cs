using ModernApp.Application.Features.TodoItems;
using ModernApp.Application.Features.TodoItems.Commands;
using ModernApp.Domain.TodoItems;
using NSubstitute;

namespace ModernApp.Application.Tests.Features.TodoItems.Commands;

public sealed class CreateTodoItemCommandHandlerTests
{
    private readonly ITodoItemRepository _repository;
    private readonly CreateTodoItemCommandHandler _sut;

    public CreateTodoItemCommandHandlerTests()
    {
        _repository = Substitute.For<ITodoItemRepository>();
        _sut = new CreateTodoItemCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesItemAndCallsRepository()
    {
        var command = new CreateTodoItemCommand("Buy groceries", "Milk, Eggs");

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        Assert.Equal("Buy groceries", result.Title);
        Assert.Equal("Milk, Eggs", result.Description);
        Assert.False(result.IsCompleted);
        Assert.NotEmpty(result.Id);
        await _repository.Received(1).AddAsync(Arg.Any<TodoItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CommandWithoutDescription_CreatesItemWithNullDescription()
    {
        var command = new CreateTodoItemCommand("Write tests", null);

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        Assert.Null(result.Description);
        await _repository.Received(1).AddAsync(Arg.Any<TodoItem>(), Arg.Any<CancellationToken>());
    }
}
