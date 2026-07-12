namespace ModernApp.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides identity and equality by ID.
/// </summary>
public abstract class Entity
{
    public string Id { get; protected set; } = Guid.NewGuid().ToString();
}
