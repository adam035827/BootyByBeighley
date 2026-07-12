using Microsoft.Azure.Cosmos;
using ModernApp.Application.Features.TodoItems;
using ModernApp.Domain.TodoItems;

namespace ModernApp.Infrastructure.Repositories;

internal sealed class TodoItemRepository(CosmosClient cosmosClient) : ITodoItemRepository
{
    private const string DatabaseId = "ModernApp";
    private const string ContainerId = "todo-items";
    private const string PartitionKeyValue = "todo-items";

    private Container Container => cosmosClient.GetContainer(DatabaseId, ContainerId);

    public async Task<TodoItem?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var response = await Container.ReadItemAsync<TodoItemDocument>(
                id, new PartitionKey(PartitionKeyValue), cancellationToken: ct);
            return response.Resource.ToDomain();
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken ct = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.type = @type AND c.partitionKey = @pk")
            .WithParameter("@type", "TodoItem")
            .WithParameter("@pk", PartitionKeyValue);

        using var iterator = Container.GetItemQueryIterator<TodoItemDocument>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(PartitionKeyValue) });

        var results = new List<TodoItem>();
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(ct);
            results.AddRange(page.Select(d => d.ToDomain()));
        }

        return results;
    }

    public async Task AddAsync(TodoItem item, CancellationToken ct = default)
    {
        var document = item.ToDocument();
        await Container.CreateItemAsync(document, new PartitionKey(PartitionKeyValue), cancellationToken: ct);
    }

    public async Task UpdateAsync(TodoItem item, CancellationToken ct = default)
    {
        var document = item.ToDocument();
        await Container.ReplaceItemAsync(document, document.Id, new PartitionKey(PartitionKeyValue), cancellationToken: ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await Container.DeleteItemAsync<TodoItemDocument>(
            id, new PartitionKey(PartitionKeyValue), cancellationToken: ct);
    }
}
