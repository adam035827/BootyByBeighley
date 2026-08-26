using ModernApp.Application.Common.Interfaces;
using ModernApp.Application.Features.PlanEnrollment.Queries;
using ModernApp.Application.Features.Users.Queries;
using ModernApp.Application.Features.WorkoutLogging.Queries;

namespace ModernApp.Api.Features;

public static class StudentEndpoints
{
    public static void MapStudentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Student");

        // GET /api/v1/my/plans
        group.MapGet("/my/plans", GetMyPlans)
            .WithName("GetMyPlans")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<List<StudentPlanDto>>();

        // GET /api/v1/students/{studentId}
        group.MapGet("/students/{studentId:guid}", GetStudentDetails)
            .WithName("GetStudentDetails")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<StudentDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetMyPlans(
        HttpContext http,
        IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>> handler)
    {
        var studentIdClaim = http.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(studentIdClaim, out var studentId))
            return Results.Unauthorized();

        var result = await handler.ExecuteAsync(
            new GetStudentPlansQuery(studentId), http.RequestAborted);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetStudentDetails(
        Guid studentId,
        IQueryHandler<GetStudentQuery, StudentDetailsDto?> handler,
        HttpContext http)
    {
        var result = await handler.ExecuteAsync(
            new GetStudentQuery(studentId), http.RequestAborted);

        return result == null
            ? Results.NotFound()
            : Results.Ok(result);
    }
}
