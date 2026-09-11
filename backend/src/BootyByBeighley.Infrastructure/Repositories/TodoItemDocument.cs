using BootyByBeighley.Infrastructure.Persistence;

namespace BootyByBeighley.Infrastructure.Repositories;

internal sealed class TodoItemDocument : CosmosDocument
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // All todo items share one logical partition — suitable for a small example.
    // Use a more granular partition key (e.g. userId) in production.
    public override string PartitionKey => "todo-items";

    public TodoItemDocument() { Type = "TodoItem"; }
}
