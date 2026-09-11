using BootyByBeighley.Api.Features;
using BootyByBeighley.Application;
using BootyByBeighley.Infrastructure;
using BootyByBeighley.Infrastructure.Persistence;
using BootyByBeighley.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Apply migrations and seed data in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await SeedData.SeedAsync(context);
    }

    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.UseStatusCodePages();

// Map all feature endpoints
app.MapAuthEndpoints();
app.MapStudentEndpoints();
app.MapCoachEndpoints();
app.MapProgressEndpoints();

app.Run();
