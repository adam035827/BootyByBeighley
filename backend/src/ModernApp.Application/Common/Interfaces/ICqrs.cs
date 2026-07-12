namespace ModernApp.Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands (write operations).
/// Commands return a response. For fire-and-forget, use ICommand with Unit.
/// </summary>
public interface ICommand<TResponse> { }

/// <summary>
/// Marker interface for queries (read operations). Must not mutate state.
/// </summary>
public interface IQuery<TResponse> { }

/// <summary>
/// Handler for a command. Implement this to handle write operations.
/// </summary>
public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> ExecuteAsync(TCommand command, CancellationToken ct);
}

/// <summary>
/// Handler for a query. Implement this to handle read operations.
/// </summary>
public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> ExecuteAsync(TQuery query, CancellationToken ct);
}
