using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Application.Features.Users.Commands;
using BootyByBeighley.Application.Features.Users.Queries;

namespace BootyByBeighley.Api.Features;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        // POST /api/v1/auth/register
        group.MapPost("/register", RegisterStudent)
            .WithName("RegisterStudent")
            .WithOpenApi()
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // GET /api/v1/auth/me
        group.MapGet("/me", GetCurrentStudent)
            .WithName("GetCurrentStudent")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<StudentDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> RegisterStudent(
        RegisterStudentCommand cmd,
        ICommandHandler<RegisterStudentCommand, UserDto> handler,
        HttpContext http)
    {
        try
        {
            var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
            return Results.Created($"/api/v1/students/{result.Id}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Registration failed");
        }
    }

    private static async Task<IResult> GetCurrentStudent(
        HttpContext http,
        IQueryHandler<GetStudentQuery, StudentDetailsDto?> handler)
    {
        var studentIdClaim = http.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(studentIdClaim, out var studentId))
            return Results.Unauthorized();

        var result = await handler.ExecuteAsync(
            new GetStudentQuery(studentId), http.RequestAborted);

        return result == null
            ? Results.NotFound()
            : Results.Ok(result);
    }
}
