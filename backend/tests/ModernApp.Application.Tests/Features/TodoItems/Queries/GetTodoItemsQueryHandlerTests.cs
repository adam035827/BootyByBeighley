using ModernApp.Application.Features.TodoItems;
using ModernApp.Application.Features.TodoItems.Queries;
using ModernApp.Domain.TodoItems;
using NSubstitute;

namespace ModernApp.Application.Tests.Features.TodoItems.Queries;

public sealed class GetTodoItemsQueryHandlerTests
{
    private readonly ITodoItemRepository _repository;
    private readonly GetTodoItemsQueryHandler _sut;

    public GetTodoItemsQueryHandlerTests()
    {
        _repository = Substitute.For<ITodoItemRepository>();
        _sut = new GetTodoItemsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ItemsExist_ReturnsAllMappedDtos()
    {
        IReadOnlyList<TodoItem> items =
        [
            TodoItem.Create("Item 1"),
            TodoItem.Create("Item 2"),
            TodoItem.Create("Item 3"),
        ];
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(items));

        var result = await _sut.ExecuteAsync(new GetTodoItemsQuery(), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(["Item 1", "Item 2", "Item 3"], result.Select(r => r.Title));
    }

    [Fact]
    public async Task Handle_NoItems_ReturnsEmptyList()
    {
        IReadOnlyList<TodoItem> empty = [];
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(empty));

        var result = await _sut.ExecuteAsync(new GetTodoItemsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
