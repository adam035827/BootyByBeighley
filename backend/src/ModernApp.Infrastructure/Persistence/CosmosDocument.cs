namespace ModernApp.Infrastructure.Persistence;

/// <summary>
/// Cosmos DB document base. All persisted documents must inherit this
/// to ensure consistent partition key and type discriminator fields.
/// Internal to Infrastructure — never reference this type from Application or Domain.
/// </summary>
internal abstract class CosmosDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Partition key value. Set by the concrete document type.
    /// Choose a partition key that distributes reads/writes evenly across logical partitions.
    /// </summary>
    public abstract string PartitionKey { get; }

    /// <summary>
    /// Type discriminator — enables polymorphic queries within a shared container.
    /// </summary>
    public string Type { get; protected set; } = string.Empty;
}
