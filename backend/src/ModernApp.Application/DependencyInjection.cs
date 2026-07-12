using Microsoft.Extensions.DependencyInjection;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Application.Features.TodoItems;
using ModernApp.Application.Features.TodoItems.Commands;
using ModernApp.Application.Features.TodoItems.Queries;

namespace ModernApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetTodoItemsQuery, IReadOnlyList<TodoItemDto>>, GetTodoItemsQueryHandler>();
        services.AddScoped<IQueryHandler<GetTodoItemQuery, TodoItemDto?>, GetTodoItemQueryHandler>();
        services.AddScoped<ICommandHandler<CreateTodoItemCommand, TodoItemDto>, CreateTodoItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTodoItemCommand, TodoItemDto?>, UpdateTodoItemCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTodoItemCommand, bool>, DeleteTodoItemCommandHandler>();

        return services;
    }
}
