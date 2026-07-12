namespace ModernApp.Api.Middleware;

/// <summary>
/// Maps unhandled exceptions to ProblemDetails responses.
/// Registered via app.UseExceptionHandler() in Program.cs.
/// .NET 10 built-in exception handler is used — this file documents
/// how to extend it with custom exception types and error codes.
/// </summary>
/// <remarks>
/// To add custom exception mappings, register an IExceptionHandler:
///
///   builder.Services.AddExceptionHandler&lt;CustomExceptionHandler&gt;();
///
/// Custom handlers run before the default ProblemDetails fallback.
/// </remarks>
public static class ExceptionHandlerExtensions
{
    // Placeholder — custom IExceptionHandler implementations go here
    // when specific exception types need distinct ProblemDetails shapes
    // (e.g., NotFoundException → 404, ConflictException → 409).
}
