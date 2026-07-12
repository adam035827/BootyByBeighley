using ModernApp.Application.Features.TodoItems;
using ModernApp.Application.Features.TodoItems.Queries;
using ModernApp.Domain.TodoItems;
using NSubstitute;

namespace ModernApp.Application.Tests.Features.TodoItems.Queries;

public sealed class GetTodoItemQueryHandlerTests
{
    private readonly ITodoItemRepository _repository;
    private readonly GetTodoItemQueryHandler _sut;

    public GetTodoItemQueryHandlerTests()
    {
        _repository = Substitute.For<ITodoItemRepository>();
        _sut = new GetTodoItemQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingItem_ReturnsMappedDto()
    {
        var item = TodoItem.Create("Buy bread", "Whole grain");
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TodoItem?>(item));

        var result = await _sut.ExecuteAsync(new GetTodoItemQuery(item.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(item.Id, result!.Id);
        Assert.Equal("Buy bread", result.Title);
        Assert.Equal("Whole grain", result.Description);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task Handle_NonExistentItem_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TodoItem?>(null));

        var result = await _sut.ExecuteAsync(new GetTodoItemQuery("unknown-id"), CancellationToken.None);

        Assert.Null(result);
    }
}
