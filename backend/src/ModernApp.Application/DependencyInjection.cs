using Microsoft.Extensions.DependencyInjection;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Application.Features.Movements.Commands;
using ModernApp.Application.Features.Movements.Queries;
using ModernApp.Application.Features.PlanEnrollment.Commands;
using ModernApp.Application.Features.PlanEnrollment.Queries;
using ModernApp.Application.Features.Users.Commands;
using ModernApp.Application.Features.Users.Queries;
using ModernApp.Application.Features.WorkoutLogging.Commands;
using ModernApp.Application.Features.WorkoutLogging.Queries;
using ModernApp.Application.Features.WorkoutPlans.Commands;
using ModernApp.Application.Features.WorkoutPlans.Queries;

namespace ModernApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // User Commands
        services.AddScoped<ICommandHandler<RegisterStudentCommand, UserDto>, RegisterStudentCommandHandler>();

        // User Queries
        services.AddScoped<IQueryHandler<GetStudentQuery, StudentDetailsDto?>, GetStudentQueryHandler>();
        services.AddScoped<IQueryHandler<GetStudentsForCoachQuery, List<StudentRosterItemDto>>, GetStudentsForCoachQueryHandler>();

        // Workout Plan Commands
        services.AddScoped<ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto>, CreateWorkoutPlanCommandHandler>();
        services.AddScoped<ICommandHandler<PublishWorkoutPlanCommand, bool>, PublishWorkoutPlanCommandHandler>();

        // Workout Plan Queries
        services.AddScoped<IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDetailDto?>, GetWorkoutPlanQueryHandler>();
        services.AddScoped<IQueryHandler<GetWorkoutPlansForCoachQuery, List<WorkoutPlanItemDto>>, GetWorkoutPlansForCoachQueryHandler>();

        // Movement Commands
        services.AddScoped<ICommandHandler<CreateMovementCommand, MovementDto>, CreateMovementCommandHandler>();
        services.AddScoped<ICommandHandler<UploadMovementVideoCommand, MovementVideoUploadDto>, UploadMovementVideoCommandHandler>();

        // Movement Queries
        services.AddScoped<IQueryHandler<GetMovementsQuery, List<MovementItemDto>>, GetMovementsQueryHandler>();

        // Plan Enrollment Commands
        services.AddScoped<ICommandHandler<EnrollStudentToPlanCommand, PlanEnrollmentDto>, EnrollStudentToPlanCommandHandler>();

        // Plan Enrollment Queries
        services.AddScoped<IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>>, GetStudentPlansQueryHandler>();

        // Workout Logging Commands
        services.AddScoped<ICommandHandler<LogWorkoutCompletionCommand, WorkoutLogEntryDto>, LogWorkoutCompletionCommandHandler>();
        services.AddScoped<ICommandHandler<SubmitWorkoutFeedbackCommand, WorkoutFeedbackDto>, SubmitWorkoutFeedbackCommandHandler>();

        // Workout Logging Queries
        services.AddScoped<IQueryHandler<GetStudentProgressQuery, StudentProgressDto>, GetStudentProgressQueryHandler>();
        services.AddScoped<IQueryHandler<GetRecentActivityQuery, List<ActivityItemDto>>, GetRecentActivityQueryHandler>();

        return services;
    }
}
