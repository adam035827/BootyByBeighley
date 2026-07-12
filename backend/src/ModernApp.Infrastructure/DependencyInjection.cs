using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernApp.Application.Features.TodoItems;
using ModernApp.Infrastructure.Repositories;

namespace ModernApp.Infrastructure;

public static class DependencyInjection
{
    private const string DatabaseId = "ModernApp";
    private const string TodoItemsContainerId = "todo-items";

    /// <summary>
    /// Registers all Infrastructure dependencies.
    /// This is the single entry point for persistence wiring — to swap the database,
    /// replace the CosmosClient registration and TodoItemRepository below.
    /// Nothing in Application or Domain needs to change.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Cosmos:ConnectionString"];

        if (connectionString is not null)
        {
            services.AddSingleton(_ => new CosmosClient(connectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            }));

            // Ensure the Cosmos database and containers exist on startup.
            services.AddHostedService<CosmosInitializer>();
        }
        else
        {
            // Deferred error: throw only when CosmosClient is first resolved at runtime.
            services.AddSingleton<CosmosClient>(_ => throw new InvalidOperationException(
                "Cosmos:ConnectionString is not configured. " +
                "Set it via user secrets (development) or environment variable / Key Vault (production)."));
        }

        services.AddScoped<ITodoItemRepository, TodoItemRepository>();

        return services;
    }

    private sealed class CosmosInitializer(CosmosClient client) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var db = await client.CreateDatabaseIfNotExistsAsync(DatabaseId, cancellationToken: cancellationToken);
            await db.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties(TodoItemsContainerId, partitionKeyPath: "/partitionKey"),
                cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
