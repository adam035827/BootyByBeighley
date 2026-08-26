using ModernApp.Application.Common.Interfaces;
using ModernApp.Application.Features.WorkoutLogging.Queries;

namespace ModernApp.Api.Features;

public static class ProgressEndpoints
{
    public static void MapProgressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/my")
            .WithTags("Progress")
            .RequireAuthorization();

        // GET /api/v1/my/progress
        group.MapGet("/progress", GetMyProgress)
            .WithName("GetMyProgress")
            .WithOpenApi()
            .Produces<StudentProgressDto>();
    }

    private static async Task<IResult> GetMyProgress(
        HttpContext http,
        IQueryHandler<GetStudentProgressQuery, StudentProgressDto> handler)
    {
        var studentIdClaim = http.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(studentIdClaim, out var studentId))
            return Results.Unauthorized();

        var result = await handler.ExecuteAsync(
            new GetStudentProgressQuery(studentId), http.RequestAborted);

        return Results.Ok(result);
    }
}
