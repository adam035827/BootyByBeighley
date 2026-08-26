using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Infrastructure.Persistence;
using ModernApp.Infrastructure.Repositories;
using ModernApp.Infrastructure.Storage;

namespace ModernApp.Infrastructure;

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
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();
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

