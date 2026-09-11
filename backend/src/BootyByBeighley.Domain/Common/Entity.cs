namespace BootyByBeighley.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides identity and equality by ID.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
