using ModernApp.Domain;
using ModernApp.Domain.PlanEnrollments;
using ModernApp.Domain.Users;
using ModernApp.Domain.WorkoutLogs;
using ModernApp.Domain.WorkoutPlans;
using ModernApp.Domain.Movements;
using ModernApp.Infrastructure.Persistence;

namespace ModernApp.Infrastructure.Seeding;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if database is empty
        if (context.Users.Any())
            return;

        // Create a coach
        var coach = User.CreateCoach("coach@bootyfitness.com", "Coach", "Beighley");
        context.Users.Add(coach);

        // Create sample students
        var students = new List<User>();
        for (int i = 1; i <= 8; i++)
        {
            var student = User.CreateStudent($"student{i}@example.com", $"Student{i}", $"User{i}");
            students.Add(student);
            context.Users.Add(student);
        }

        await context.SaveChangesAsync();

        // Create sample workout plan
        var plan = WorkoutPlan.Create(
            name: "Beginner Strength - Phase 1",
            difficulty: Difficulty.Beginner,
            durationWeeks: 8,
            trainingFrequencyDaysPerWeek: 3,
            description: "A foundational strength program for beginners");

        context.WorkoutPlans.Add(plan);

        // Create sample movement
        var movement = Movement.Create(
            name: "Push-up",
            defaultSets: 3,
            defaultReps: 10,
            description: "Classic bodyweight push-up",
            defaultRestSeconds: 60);

        // Set video details via reflection since they're private
        movement.GetType().GetProperty("VideoUrl")?.SetValue(movement, "https://example.com/videos/pushup.mp4");
        movement.GetType().GetProperty("VideoCaption")?.SetValue(movement, "Keep your body straight and lower yourself until chest nearly touches the ground.");

        context.Movements.Add(movement);

        await context.SaveChangesAsync();

        // Create a workout
        var workout = Workout.Create(
            workoutPlanId: plan.Id,
            name: "Upper Body Day 1",
            order: 1,
            estimatedDurationMinutes: 45,
            description: "Focus on chest and shoulders");

        context.Workouts.Add(workout);

        // Create workout movement
        var workoutMovement = WorkoutMovement.Create(
            workoutId: workout.Id,
            movementId: movement.Id,
            order: 1,
            prescribedSets: 3,
            prescribedReps: 10,
            prescribedRestSeconds: 60);

        context.WorkoutMovements.Add(workoutMovement);

        await context.SaveChangesAsync();

        // Enroll students in the plan
        var enrollments = new List<PlanEnrollment>();
        var selectedDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

        foreach (var student in students.Take(6))
        {
            var enrollment = PlanEnrollment.Create(
                userId: student.Id,
                workoutPlanId: plan.Id,
                selectedTrainingDays: selectedDays,
                durationWeeks: 8);

            enrollments.Add(enrollment);
            context.PlanEnrollments.Add(enrollment);
        }

        await context.SaveChangesAsync();

        // Create sample workout logs (workouts completed)
        var workoutStartDate = DateTime.UtcNow.AddDays(-20);
        var enrollmentsToLog = enrollments.Take(4).ToList();

        foreach (var enrollment in enrollmentsToLog)
        {
            // 2-4 completed workouts per student
            for (int i = 0; i < Random.Shared.Next(2, 5); i++)
            {
                var logDate = workoutStartDate.AddDays(i * 4 + Random.Shared.Next(0, 3));

                // Create set entries
                var setEntries = new List<WorkoutLogSetEntry>();
                for (int setNum = 0; setNum < 3; setNum++)
                {
                    var setEntry = WorkoutLogSetEntry.Create(
                        workoutLogEntryId: Guid.NewGuid(), // Placeholder, will be set in next version
                        workoutMovementId: workoutMovement.Id,
                        setNumber: setNum + 1,
                        repsCompleted: 10 + Random.Shared.Next(-2, 3),
                        weightUsed: 10 + (setNum * 2.5m));

                    setEntries.Add(setEntry);
                }

                // Create the log entry
                var logEntry = WorkoutLogEntry.CreateCompleted(
                    userId: enrollment.UserId,
                    workoutId: workout.Id,
                    planEnrollmentId: enrollment.Id,
                    loggedSets: setEntries,
                    notes: i % 2 == 0 ? "Great workout, felt strong!" : null);

                context.WorkoutLogEntries.Add(logEntry);
            }
        }

        await context.SaveChangesAsync();
    }
}
