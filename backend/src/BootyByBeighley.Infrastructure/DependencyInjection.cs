using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Infrastructure.Persistence;
using BootyByBeighley.Infrastructure.Repositories;
using BootyByBeighley.Infrastructure.Storage;

namespace BootyByBeighley.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure dependencies.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("BootyByBeighley.Infrastructure")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
        services.AddScoped<IPlanEnrollmentRepository, PlanEnrollmentRepository>();
        services.AddScoped<IWorkoutLogRepository, WorkoutLogRepository>();
        services.AddScoped<IPersonalRecordRepository, PersonalRecordRepository>();

        // Azure Blob Storage
        var blobContainerUri = configuration["Azure:BlobStorage:ContainerUri"]
            ?? throw new InvalidOperationException("Azure:BlobStorage:ContainerUri not configured");

        services.AddSingleton(new BlobContainerClient(
            new Uri(blobContainerUri),
            new DefaultAzureCredential()));

        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }
}

