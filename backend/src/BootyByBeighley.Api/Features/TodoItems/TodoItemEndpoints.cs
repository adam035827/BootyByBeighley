using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Application.Features.TodoItems;
using BootyByBeighley.Application.Features.TodoItems.Commands;
using BootyByBeighley.Application.Features.TodoItems.Queries;

namespace BootyByBeighley.Api.Features.TodoItems;

public static class TodoItemEndpoints
{
    public static IEndpointRouteBuilder MapTodoItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todo-items")
            .WithTags("TodoItems")
            .AllowAnonymous();

        group.MapGet("/", async (
            IQueryHandler<GetTodoItemsQuery, IReadOnlyList<TodoItemDto>> handler,
            CancellationToken ct) =>
        {
            var query = new GetTodoItemsQuery();
            return await handler.ExecuteAsync(query, ct);
        })
        .WithName("GetTodoItems")
        .Produces<IReadOnlyList<TodoItemDto>>();

        group.MapGet("/{id}", async (
            string id,
            IQueryHandler<GetTodoItemQuery, TodoItemDto?> handler,
            CancellationToken ct) =>
        {
            var query = new GetTodoItemQuery(id);
            var item = await handler.ExecuteAsync(query, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("GetTodoItem")
        .Produces<TodoItemDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            CreateTodoItemCommand command,
            ICommandHandler<CreateTodoItemCommand, TodoItemDto> handler,
            CancellationToken ct) =>
        {
            var item = await handler.ExecuteAsync(command, ct);
            return Results.CreatedAtRoute("GetTodoItem", new { id = item.Id }, item);
        })
        .WithName("CreateTodoItem")
        .Produces<TodoItemDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapPut("/{id}", async (
            string id,
            UpdateTodoItemRequest body,
            ICommandHandler<UpdateTodoItemCommand, TodoItemDto?> handler,
            CancellationToken ct) =>
        {
            var command = new UpdateTodoItemCommand(id, body.Title, body.Description, body.IsCompleted);
            var item = await handler.ExecuteAsync(command, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("UpdateTodoItem")
        .Produces<TodoItemDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();

        group.MapDelete("/{id}", async (
            string id,
            ICommandHandler<DeleteTodoItemCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new DeleteTodoItemCommand(id);
            var deleted = await handler.ExecuteAsync(command, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteTodoItem")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
