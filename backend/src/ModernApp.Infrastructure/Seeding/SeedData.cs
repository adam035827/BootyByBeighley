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
        for (int i = 1; i <= 12; i++)
        {
            var student = User.CreateStudent($"student{i}@example.com", $"Student{i}", $"User{i}");
            students.Add(student);
            context.Users.Add(student);
        }

        await context.SaveChangesAsync();

        // Create sample workout plans
        var plan1 = WorkoutPlan.Create(
            name: "Beginner Strength - Phase 1",
            difficulty: Difficulty.Beginner,
            durationWeeks: 8,
            trainingFrequencyDaysPerWeek: 3,
            description: "A foundational strength program for beginners");

        var plan2 = WorkoutPlan.Create(
            name: "Intermediate Hypertrophy",
            difficulty: Difficulty.Intermediate,
            durationWeeks: 12,
            trainingFrequencyDaysPerWeek: 4,
            description: "Build muscle mass with higher volume training");

        var plan3 = WorkoutPlan.Create(
            name: "Advanced Powerlifting",
            difficulty: Difficulty.Advanced,
            durationWeeks: 16,
            trainingFrequencyDaysPerWeek: 5,
            description: "Build maximal strength focusing on the big three");

        context.WorkoutPlans.AddRange(plan1, plan2, plan3);

        // Create sample movements
        var movements = new List<Movement>();
        var movementData = new (string name, int sets, int reps, string desc, string caption)[]
        {
            ("Push-up", 3, 10, "Classic bodyweight push-up", "Keep your body straight and lower yourself until chest nearly touches the ground."),
            ("Squat", 4, 12, "Bodyweight squat", "Keep knees tracking over toes, lower until thighs are parallel to ground."),
            ("Deadlift", 5, 5, "Barbell deadlift", "Maintain neutral spine, drive through heels, keep bar close to body."),
            ("Bench Press", 4, 8, "Barbell bench press", "Control the descent, drive through chest to lockout."),
            ("Barbell Row", 4, 8, "Barbell bent-over row", "Maintain neutral spine, pull bar to chest, control the descent."),
            ("Pull-up", 3, 8, "Bodyweight pull-up", "Full range of motion from dead hang to chin over bar."),
            ("Lunges", 3, 10, "Walking lunges", "Step forward, lower until back knee nearly touches ground."),
            ("Plank", 3, 45, "Forearm plank", "Keep body perfectly straight from head to heels for prescribed time."),
        };

        foreach (var (name, sets, reps, desc, caption) in movementData)
        {
            var movement = Movement.Create(
                name: name,
                defaultSets: sets,
                defaultReps: reps,
                description: desc,
                defaultRestSeconds: 60);

            movement.GetType().GetProperty("VideoUrl")?.SetValue(movement, $"https://example.com/videos/{name.ToLower().Replace(" ", "-")}.mp4");
            movement.GetType().GetProperty("VideoCaption")?.SetValue(movement, caption);

            movements.Add(movement);
            context.Movements.Add(movement);
        }

        await context.SaveChangesAsync();

        // Create workouts for each plan
        var workouts = new List<Workout>();

        // Plan 1 workouts
        var plan1Workouts = new[]
        {
            ("Upper Body Day 1", "Focus on chest and shoulders", 45),
            ("Lower Body Day 1", "Focus on quads and glutes", 50),
            ("Full Body Day 1", "Balanced strength training", 60),
        };

        foreach (var (name, desc, duration) in plan1Workouts)
        {
            var workout = Workout.Create(workoutPlanId: plan1.Id, name: name, order: workouts.Count + 1, estimatedDurationMinutes: duration, description: desc);
            workouts.Add(workout);
            context.Workouts.Add(workout);
        }

        // Plan 2 workouts
        var plan2Workouts = new[]
        {
            ("Push Day", "Chest, shoulders, triceps", 60),
            ("Pull Day", "Back, biceps, rear delts", 60),
            ("Leg Day", "Quads, hamstrings, glutes", 75),
            ("Upper Power", "Low reps, heavy weight", 50),
        };

        foreach (var (name, desc, duration) in plan2Workouts)
        {
            var workout = Workout.Create(workoutPlanId: plan2.Id, name: name, order: workouts.Count + 1, estimatedDurationMinutes: duration, description: desc);
            workouts.Add(workout);
            context.Workouts.Add(workout);
        }

        // Plan 3 workouts
        var plan3Workouts = new[]
        {
            ("Squat Day", "Squat, assistance", 90),
            ("Bench Day", "Bench, assistance", 85),
            ("Deadlift Day", "Deadlift, assistance", 90),
            ("Speed Squat", "Dynamic effort squat", 60),
            ("Speed Bench", "Dynamic effort bench", 60),
        };

        foreach (var (name, desc, duration) in plan3Workouts)
        {
            var workout = Workout.Create(workoutPlanId: plan3.Id, name: name, order: workouts.Count + 1, estimatedDurationMinutes: duration, description: desc);
            workouts.Add(workout);
            context.Workouts.Add(workout);
        }

        await context.SaveChangesAsync();

        // Assign movements to workouts
        var workoutMovements = new List<WorkoutMovement>();
        for (int i = 0; i < workouts.Count; i++)
        {
            // Each workout gets 3-4 movements
            for (int m = 0; m < Random.Shared.Next(3, 5); m++)
            {
                var movement = movements[Random.Shared.Next(movements.Count)];
                var workoutMovement = WorkoutMovement.Create(
                    workoutId: workouts[i].Id,
                    movementId: movement.Id,
                    order: m + 1,
                    prescribedSets: movement.DefaultSets,
                    prescribedReps: movement.DefaultReps,
                    prescribedRestSeconds: 60);

                workoutMovements.Add(workoutMovement);
                context.WorkoutMovements.Add(workoutMovement);
            }
        }

        await context.SaveChangesAsync();

        // Enroll students in the plans
        var enrollments = new List<PlanEnrollment>();
        var selectedDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
        var selectedDays4x = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        // Plan 1: First 6 students
        foreach (var student in students.Take(6))
        {
            var enrollment = PlanEnrollment.Create(
                userId: student.Id,
                workoutPlanId: plan1.Id,
                selectedTrainingDays: selectedDays,
                durationWeeks: 8);

            enrollments.Add(enrollment);
            context.PlanEnrollments.Add(enrollment);
        }

        // Plan 2: Students 4-10
        foreach (var student in students.Skip(3).Take(7))
        {
            var enrollment = PlanEnrollment.Create(
                userId: student.Id,
                workoutPlanId: plan2.Id,
                selectedTrainingDays: selectedDays4x,
                durationWeeks: 12);

            enrollments.Add(enrollment);
            context.PlanEnrollments.Add(enrollment);
        }

        // Plan 3: Students 7-12
        foreach (var student in students.Skip(6).Take(6))
        {
            var enrollment = PlanEnrollment.Create(
                userId: student.Id,
                workoutPlanId: plan3.Id,
                selectedTrainingDays: selectedDays4x,
                durationWeeks: 16);

            enrollments.Add(enrollment);
            context.PlanEnrollments.Add(enrollment);
        }

        await context.SaveChangesAsync();

        // Create detailed workout logs
        var workoutStartDate = DateTime.UtcNow.AddDays(-30);

        foreach (var enrollment in enrollments.Take(10))
        {
            // 5-10 completed workouts per enrollment
            for (int i = 0; i < Random.Shared.Next(5, 11); i++)
            {
                var logDate = workoutStartDate.AddDays(i * 2 + Random.Shared.Next(0, 2));

                // Pick a random workout from the plan
                var planWorkouts = workouts.Where(w => w.WorkoutPlanId == enrollment.WorkoutPlanId).ToList();
                var workout = planWorkouts[Random.Shared.Next(planWorkouts.Count)];

                // Get movements for this workout
                var workoutMovementList = context.WorkoutMovements.Where(wm => wm.WorkoutId == workout.Id).ToList();

                var setEntries = new List<WorkoutLogSetEntry>();
                foreach (var wm in workoutMovementList)
                {
                    for (int setNum = 0; setNum < wm.PrescribedSets; setNum++)
                    {
                        var repsVariation = Random.Shared.Next(-3, 4);
                        var setEntry = WorkoutLogSetEntry.Create(
                            workoutLogEntryId: Guid.NewGuid(),
                            workoutMovementId: wm.Id,
                            setNumber: setNum + 1,
                            repsCompleted: Math.Max(1, wm.PrescribedReps + repsVariation),
                            weightUsed: 10 + (setNum * 2.5m) + (decimal)Random.Shared.Next(0, 10));

                        setEntries.Add(setEntry);
                    }
                }

                var notes = new[] { "Great workout!", "Felt strong today", "Need more sleep", "PR on this exercise!", "Good form, maintained tempo", null };
                var logEntry = WorkoutLogEntry.CreateCompleted(
                    userId: enrollment.UserId,
                    workoutId: workout.Id,
                    planEnrollmentId: enrollment.Id,
                    loggedSets: setEntries,
                    notes: notes[Random.Shared.Next(notes.Length)]);

                context.WorkoutLogEntries.Add(logEntry);
            }
        }

        await context.SaveChangesAsync();
    }
}
