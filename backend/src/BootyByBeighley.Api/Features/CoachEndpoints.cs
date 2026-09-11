using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Application.Features.Movements.Commands;
using BootyByBeighley.Application.Features.Movements.Queries;
using BootyByBeighley.Application.Features.PlanEnrollment.Commands;
using BootyByBeighley.Application.Features.PlanEnrollment.Queries;
using BootyByBeighley.Application.Features.Users.Queries;
using BootyByBeighley.Application.Features.WorkoutLogging.Queries;
using BootyByBeighley.Application.Features.WorkoutPlans.Commands;
using BootyByBeighley.Application.Features.WorkoutPlans.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BootyByBeighley.Api.Features;

public static class CoachEndpoints
{
    public static void MapCoachEndpoints(this WebApplication app)
    {
        MapStudentsEndpoints(app);
        MapCoachDashboardDataEndpoints(app);
        MapWorkoutPlanEndpoints(app);
        MapMovementEndpoints(app);
        MapPlanEnrollmentEndpoints(app);
    }

    private static void MapStudentsEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/coach/students")
            .WithTags("Students");
            // TODO: Add role-based authorization once auth system is implemented
            // .RequireAuthorization(policy => policy.RequireRole("Coach"));

        // GET /api/v1/coach/students
        group.MapGet("/", GetStudentsRoster)
            .WithName("GetStudentsRoster")
            .WithOpenApi()
            .Produces<List<StudentRosterItemDto>>();
    }

    private static void MapCoachDashboardDataEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/coach")
            .WithTags("Coach Dashboard");
            // TODO: Add role-based authorization once auth system is implemented
            // .RequireAuthorization(policy => policy.RequireRole("Coach"));

        // GET /api/v1/coach/plans
        group.MapGet("/plans", GetWorkoutPlans)
            .WithName("GetCoachWorkoutPlans")
            .WithOpenApi()
            .Produces<List<WorkoutPlanItemDto>>();

        // GET /api/v1/coach/movements
        group.MapGet("/movements", GetMovements)
            .WithName("GetMovements")
            .WithOpenApi()
            .Produces<List<MovementItemDto>>();

        // GET /api/v1/coach/activity
        group.MapGet("/activity", GetRecentActivity)
            .WithName("GetRecentActivity")
            .WithOpenApi()
            .Produces<List<ActivityItemDto>>();
    }

    private static async Task<IResult> GetStudentsRoster(
        HttpContext http,
        [FromServices] IQueryHandler<GetStudentsForCoachQuery, List<StudentRosterItemDto>> handler)
    {
        var result = await handler.ExecuteAsync(new GetStudentsForCoachQuery(), http.RequestAborted);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetWorkoutPlans(
        HttpContext http,
        [FromServices] IQueryHandler<GetWorkoutPlansForCoachQuery, List<WorkoutPlanItemDto>> handler)
    {
        var result = await handler.ExecuteAsync(new GetWorkoutPlansForCoachQuery(), http.RequestAborted);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetMovements(
        HttpContext http,
        [FromServices] IQueryHandler<GetMovementsQuery, List<MovementItemDto>> handler)
    {
        var result = await handler.ExecuteAsync(new GetMovementsQuery(), http.RequestAborted);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRecentActivity(
        int limitDays = 7,
        HttpContext? http = null,
        [FromServices] IQueryHandler<GetRecentActivityQuery, List<ActivityItemDto>>? handler = null)
    {
        if (handler == null || http == null)
            return Results.BadRequest("Handler or context not available");

        var result = await handler.ExecuteAsync(new GetRecentActivityQuery(limitDays), http.RequestAborted);
        return Results.Ok(result);
    }

    private static void MapWorkoutPlanEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workout-plans")
            .WithTags("WorkoutPlans")
            .RequireAuthorization(policy => policy.RequireRole("Coach"));

        // POST /api/v1/workout-plans
        group.MapPost("/", CreateWorkoutPlan)
            .WithName("CreateWorkoutPlan")
            .WithOpenApi()
            .Produces<WorkoutPlanDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // GET /api/v1/workout-plans/{planId}
        group.MapGet("/{planId:guid}", GetWorkoutPlan)
            .WithName("GetWorkoutPlan")
            .WithOpenApi()
            .Produces<WorkoutPlanDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // PUT /api/v1/workout-plans/{planId}/publish
        group.MapPut("/{planId:guid}/publish", PublishWorkoutPlan)
            .WithName("PublishWorkoutPlan")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static void MapMovementEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/movements")
            .WithTags("Movements")
            .RequireAuthorization(policy => policy.RequireRole("Coach"));

        // POST /api/v1/movements
        group.MapPost("/", CreateMovement)
            .WithName("CreateMovement")
            .WithOpenApi()
            .Produces<MovementDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // POST /api/v1/movements/{movementId}/video
        group.MapPost("/{movementId:guid}/video", UploadMovementVideo)
            .WithName("UploadMovementVideo")
            .WithOpenApi()
            .Produces<MovementVideoUploadDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // GET /api/v1/movements/{movementId}/video-url (public, requires auth)
        app.MapGroup("/api/v1/movements")
            .WithTags("Movements")
            .RequireAuthorization()
            .MapGet("/{movementId:guid}/video-url", GetMovementVideoUrl)
            .WithName("GetMovementVideoUrl")
            .WithOpenApi()
            .Produces<VideoUrlDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static void MapPlanEnrollmentEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/plan-enrollments")
            .WithTags("PlanEnrollment")
            .RequireAuthorization(policy => policy.RequireRole("Coach"));

        // POST /api/v1/plan-enrollments
        group.MapPost("/", EnrollStudentToPlan)
            .WithName("EnrollStudentToPlan")
            .WithOpenApi()
            .Produces<PlanEnrollmentDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // GET /api/v1/students/{studentId}/plan-enrollments
        group.MapGet("/students/{studentId:guid}", GetStudentPlanEnrollments)
            .WithName("GetStudentPlanEnrollments")
            .WithOpenApi()
            .Produces<List<StudentPlanDto>>();
    }

    private static async Task<IResult> CreateWorkoutPlan(
        CreateWorkoutPlanCommand cmd,
        ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto> handler,
        HttpContext http)
    {
        try
        {
            var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
            return Results.Created($"/api/v1/workout-plans/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid workout plan");
        }
    }

    private static async Task<IResult> GetWorkoutPlan(
        Guid planId,
        IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDetailDto?> handler,
        HttpContext http)
    {
        var result = await handler.ExecuteAsync(
            new GetWorkoutPlanQuery(planId), http.RequestAborted);

        return result == null
            ? Results.NotFound()
            : Results.Ok(result);
    }

    private static async Task<IResult> PublishWorkoutPlan(
        Guid planId,
        ICommandHandler<PublishWorkoutPlanCommand, bool> handler,
        HttpContext http)
    {
        try
        {
            await handler.ExecuteAsync(new PublishWorkoutPlanCommand(planId), http.RequestAborted);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Plan not found");
        }
    }

    private static async Task<IResult> CreateMovement(
        CreateMovementCommand cmd,
        ICommandHandler<CreateMovementCommand, MovementDto> handler,
        HttpContext http)
    {
        try
        {
            var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
            return Results.Created($"/api/v1/movements/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid movement");
        }
    }

    private static async Task<IResult> UploadMovementVideo(
        Guid movementId,
        HttpRequest request,
        ICommandHandler<UploadMovementVideoCommand, MovementVideoUploadDto> handler,
        HttpContext http)
    {
        try
        {
            var form = await request.ReadFormAsync(http.RequestAborted);
            var videoFile = form.Files["videoFile"];
            var caption = form["caption"].ToString();

            if (videoFile == null || videoFile.Length == 0)
                return Results.Problem(
                    detail: "Video file is required",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Missing video file");

            if (string.IsNullOrWhiteSpace(caption))
                return Results.Problem(
                    detail: "Caption is required",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Missing caption");

            using var stream = videoFile.OpenReadStream();
            var cmd = new UploadMovementVideoCommand(
                movementId, caption, stream, videoFile.FileName);

            var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Movement not found");
        }
    }

    private static async Task<IResult> GetMovementVideoUrl(
        Guid movementId,
        IMovementRepository movementRepository,
        IBlobStorageService blobStorage,
        HttpContext http)
    {
        var movement = await movementRepository.GetByIdAsync(movementId, http.RequestAborted);
        if (movement?.VideoUrl == null)
            return Results.NotFound("No video available for this movement");

        try
        {
            // For now, return the existing URL if it's already a signed URL,
            // or generate a new signed URL if needed
            var uri = new Uri(movement.VideoUrl);
            var blobName = uri.LocalPath.Trim('/').Split('/').Last();
            var signedUrl = await blobStorage.GetSignedUrlAsync(
                "movement-videos", blobName, 60, http.RequestAborted);

            return Results.Ok(new VideoUrlDto(signedUrl.ToString()));
        }
        catch (Exception)
        {
            // Return the original URL if we can't generate a signed URL
            return Results.Ok(new VideoUrlDto(movement.VideoUrl));
        }
    }

    private static async Task<IResult> EnrollStudentToPlan(
        EnrollStudentToPlanCommand cmd,
        ICommandHandler<EnrollStudentToPlanCommand, PlanEnrollmentDto> handler,
        HttpContext http)
    {
        try
        {
            var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
            return Results.Created($"/api/v1/plan-enrollments/{result.Id}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Enrollment failed");
        }
    }

    private static async Task<IResult> GetStudentPlanEnrollments(
        Guid studentId,
        IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>> handler,
        HttpContext http)
    {
        var result = await handler.ExecuteAsync(
            new GetStudentPlansQuery(studentId), http.RequestAborted);

        return Results.Ok(result);
    }
}

public record VideoUrlDto(string VideoUrl);
