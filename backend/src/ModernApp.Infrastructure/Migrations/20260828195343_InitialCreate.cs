using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DefaultSets = table.Column<int>(type: "integer", nullable: false),
                    DefaultReps = table.Column<int>(type: "integer", nullable: false),
                    DefaultRestSeconds = table.Column<int>(type: "integer", nullable: true),
                    VideoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    VideoCaption = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questionnaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QuestionNumber = table.Column<int>(type: "integer", nullable: false),
                    AnswerOptions = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionnaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubscriptionStatus = table.Column<int>(type: "integer", nullable: false),
                    ProfilePhotoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SocialLinks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HeartRateAvgBpm = table.Column<double>(type: "double precision", nullable: true),
                    HeartRateMaxBpm = table.Column<double>(type: "double precision", nullable: true),
                    CaloriesBurned = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    DurationWeeks = table.Column<int>(type: "integer", nullable: false),
                    TrainingFrequencyDaysPerWeek = table.Column<int>(type: "integer", nullable: false),
                    NextPhasePlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutPlans_WorkoutPlans_NextPhasePlanId",
                        column: x => x.NextPhasePlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedAnswer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireResponses_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnaireResponses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RuleDefinition = table.Column<string>(type: "jsonb", nullable: false),
                    TargetWorkoutPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchingRules_WorkoutPlans_TargetWorkoutPlanId",
                        column: x => x.TargetWorkoutPlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SelectedTrainingDays = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanEnrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanEnrollments_WorkoutPlans_WorkoutPlanId",
                        column: x => x.WorkoutPlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workouts_WorkoutPlans_WorkoutPlanId",
                        column: x => x.WorkoutPlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MissedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLogEntries_PlanEnrollments_PlanEnrollmentId",
                        column: x => x.PlanEnrollmentId,
                        principalTable: "PlanEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutLogEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutLogEntries_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PrescribedSets = table.Column<int>(type: "integer", nullable: false),
                    PrescribedReps = table.Column<int>(type: "integer", nullable: false),
                    PrescribedRestSeconds = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutMovements_Movements_MovementId",
                        column: x => x.MovementId,
                        principalTable: "Movements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutMovements_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissedWorkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutLogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MissedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissedWorkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissedWorkouts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissedWorkouts_WorkoutLogEntries_WorkoutLogEntryId",
                        column: x => x.WorkoutLogEntryId,
                        principalTable: "WorkoutLogEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissedWorkouts_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutFeedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutLogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DifficultyRating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutFeedback_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutFeedback_WorkoutLogEntries_WorkoutLogEntryId",
                        column: x => x.WorkoutLogEntryId,
                        principalTable: "WorkoutLogEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogSetEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutLogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutMovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    RepsCompleted = table.Column<int>(type: "integer", nullable: false),
                    WeightUsed = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    NotesPerMovement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogSetEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLogSetEntries_WorkoutLogEntries_WorkoutLogEntryId",
                        column: x => x.WorkoutLogEntryId,
                        principalTable: "WorkoutLogEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutLogSetEntries_WorkoutMovements_WorkoutMovementId",
                        column: x => x.WorkoutMovementId,
                        principalTable: "WorkoutMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxWeightKg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkoutLogSetEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalRecords_Movements_MovementId",
                        column: x => x.MovementId,
                        principalTable: "Movements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalRecords_WorkoutLogSetEntries_WorkoutLogSetEntryId",
                        column: x => x.WorkoutLogSetEntryId,
                        principalTable: "WorkoutLogSetEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchingRules_IsActive",
                table: "MatchingRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingRules_Priority",
                table: "MatchingRules",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MatchingRules_TargetWorkoutPlanId",
                table: "MatchingRules",
                column: "TargetWorkoutPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWorkouts_MissedDate",
                table: "MissedWorkouts",
                column: "MissedDate");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWorkouts_UserId",
                table: "MissedWorkouts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWorkouts_WorkoutId",
                table: "MissedWorkouts",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWorkouts_WorkoutLogEntryId",
                table: "MissedWorkouts",
                column: "WorkoutLogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Movements_CreatedAt",
                table: "Movements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Movements_Name",
                table: "Movements",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecords_LoggedAt",
                table: "PersonalRecords",
                column: "LoggedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecords_MovementId",
                table: "PersonalRecords",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecords_UserId",
                table: "PersonalRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecords_UserId_MovementId",
                table: "PersonalRecords",
                columns: new[] { "UserId", "MovementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecords_WorkoutLogSetEntryId",
                table: "PersonalRecords",
                column: "WorkoutLogSetEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanEnrollments_Status",
                table: "PlanEnrollments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PlanEnrollments_UserId",
                table: "PlanEnrollments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanEnrollments_UserId_WorkoutPlanId",
                table: "PlanEnrollments",
                columns: new[] { "UserId", "WorkoutPlanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanEnrollments_WorkoutPlanId",
                table: "PlanEnrollments",
                column: "WorkoutPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_AnsweredAt",
                table: "QuestionnaireResponses",
                column: "AnsweredAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_QuestionnaireId",
                table: "QuestionnaireResponses",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_UserId",
                table: "QuestionnaireResponses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaires_IsActive",
                table: "Questionnaires",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaires_QuestionNumber",
                table: "Questionnaires",
                column: "QuestionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutFeedback_SubmittedAt",
                table: "WorkoutFeedback",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutFeedback_UserId",
                table: "WorkoutFeedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutFeedback_WorkoutLogEntryId",
                table: "WorkoutFeedback",
                column: "WorkoutLogEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogEntries_CompletedAt",
                table: "WorkoutLogEntries",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogEntries_PlanEnrollmentId",
                table: "WorkoutLogEntries",
                column: "PlanEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogEntries_Status",
                table: "WorkoutLogEntries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogEntries_UserId",
                table: "WorkoutLogEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogEntries_WorkoutId",
                table: "WorkoutLogEntries",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogSetEntries_WorkoutLogEntryId",
                table: "WorkoutLogSetEntries",
                column: "WorkoutLogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogSetEntries_WorkoutLogEntryId_SetNumber",
                table: "WorkoutLogSetEntries",
                columns: new[] { "WorkoutLogEntryId", "SetNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogSetEntries_WorkoutMovementId",
                table: "WorkoutLogSetEntries",
                column: "WorkoutMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutMovements_MovementId",
                table: "WorkoutMovements",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutMovements_WorkoutId",
                table: "WorkoutMovements",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutMovements_WorkoutId_Order",
                table: "WorkoutMovements",
                columns: new[] { "WorkoutId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_CreatedAt",
                table: "WorkoutPlans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_IsPublished",
                table: "WorkoutPlans",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_NextPhasePlanId",
                table: "WorkoutPlans",
                column: "NextPhasePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_WorkoutPlanId",
                table: "Workouts",
                column: "WorkoutPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_WorkoutPlanId_Order",
                table: "Workouts",
                columns: new[] { "WorkoutPlanId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchingRules");

            migrationBuilder.DropTable(
                name: "MissedWorkouts");

            migrationBuilder.DropTable(
                name: "PersonalRecords");

            migrationBuilder.DropTable(
                name: "QuestionnaireResponses");

            migrationBuilder.DropTable(
                name: "TodoItems");

            migrationBuilder.DropTable(
                name: "WorkoutFeedback");

            migrationBuilder.DropTable(
                name: "WorkoutLogSetEntries");

            migrationBuilder.DropTable(
                name: "Questionnaires");

            migrationBuilder.DropTable(
                name: "WorkoutLogEntries");

            migrationBuilder.DropTable(
                name: "WorkoutMovements");

            migrationBuilder.DropTable(
                name: "PlanEnrollments");

            migrationBuilder.DropTable(
                name: "Movements");

            migrationBuilder.DropTable(
                name: "Workouts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkoutPlans");
        }
    }
}
