using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HealthSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MuscleGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Equipment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.ExerciseId);
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    FoodItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServingSize = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    ServingUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaloriesKcal = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    ProteinG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    CarbsG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    FatG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.FoodItemId);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    GoalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.GoalId);
                    table.ForeignKey(
                        name: "FK_Goals_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NutritionLogs",
                columns: table => new
                {
                    NutritionLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCalories = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    ProteinG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    CarbsG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    FatG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionLogs", x => x.NutritionLogId);
                    table.ForeignKey(
                        name: "FK_NutritionLogs_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeightCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ActivityLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogs",
                columns: table => new
                {
                    WorkoutLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WorkoutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMin = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogs", x => x.WorkoutLogId);
                    table.ForeignKey(
                        name: "FK_WorkoutLogs_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressRecords",
                columns: table => new
                {
                    ProgressRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    WaistCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressRecords", x => x.ProgressRecordId);
                    table.ForeignKey(
                        name: "FK_ProgressRecords_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodEntries",
                columns: table => new
                {
                    FoodEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NutritionLogId = table.Column<int>(type: "int", nullable: false),
                    FoodItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    MealType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaloriesKcal = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    ProteinG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    CarbsG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    FatG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodEntries", x => x.FoodEntryId);
                    table.ForeignKey(
                        name: "FK_FoodEntries_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItems",
                        principalColumn: "FoodItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodEntries_NutritionLogs_NutritionLogId",
                        column: x => x.NutritionLogId,
                        principalTable: "NutritionLogs",
                        principalColumn: "NutritionLogId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSessions",
                columns: table => new
                {
                    ExerciseSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutLogId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Sets = table.Column<int>(type: "int", nullable: false),
                    Reps = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RestSec = table.Column<int>(type: "int", nullable: true),
                    Rpe = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSessions", x => x.ExerciseSessionId);
                    table.ForeignKey(
                        name: "FK_ExerciseSessions_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseSessions_WorkoutLogs_WorkoutLogId",
                        column: x => x.WorkoutLogId,
                        principalTable: "WorkoutLogs",
                        principalColumn: "WorkoutLogId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "ExerciseId", "Description", "Difficulty", "Equipment", "MuscleGroup", "Name" },
                values: new object[,]
                {
                    { 1, "Classic bodyweight chest exercise", "Beginner", "None", "Chest", "Push-ups" },
                    { 2, "Compound chest exercise with barbell", "Intermediate", "Barbell", "Chest", "Bench Press" },
                    { 3, "Isolation exercise for chest", "Intermediate", "Dumbbells", "Chest", "Dumbbell Fly" },
                    { 4, "Bodyweight back exercise", "Intermediate", "Pull-up Bar", "Back", "Pull-ups" },
                    { 5, "Compound full-body exercise", "Advanced", "Barbell", "Back", "Deadlift" },
                    { 6, "Compound back exercise", "Intermediate", "Barbell", "Back", "Bent-over Row" },
                    { 7, "Fundamental leg exercise", "Beginner", "None", "Legs", "Squats" },
                    { 8, "Unilateral leg exercise", "Beginner", "None", "Legs", "Lunges" },
                    { 9, "Machine-based leg exercise", "Intermediate", "Machine", "Legs", "Leg Press" },
                    { 10, "Overhead pressing movement", "Intermediate", "Dumbbells", "Shoulders", "Shoulder Press" },
                    { 11, "Isolation shoulder exercise", "Beginner", "Dumbbells", "Shoulders", "Lateral Raise" },
                    { 12, "Isolation bicep exercise", "Beginner", "Dumbbells", "Arms", "Bicep Curls" },
                    { 13, "Bodyweight tricep exercise", "Intermediate", "Parallel Bars", "Arms", "Tricep Dips" },
                    { 14, "Isometric core exercise", "Beginner", "None", "Core", "Plank" },
                    { 15, "Basic abdominal exercise", "Beginner", "None", "Core", "Crunches" }
                });

            migrationBuilder.InsertData(
                table: "FoodItems",
                columns: new[] { "FoodItemId", "CaloriesKcal", "CarbsG", "FatG", "Name", "ProteinG", "ServingSize", "ServingUnit" },
                values: new object[,]
                {
                    { 1, 165m, 0m, 3.6m, "Chicken Breast", 31m, 100m, "g" },
                    { 2, 111m, 23m, 0.9m, "Brown Rice", 2.6m, 100m, "g" },
                    { 3, 105m, 27m, 0.4m, "Banana", 1.3m, 118m, "g" },
                    { 4, 100m, 6m, 0m, "Greek Yogurt", 17m, 170m, "g" },
                    { 5, 7m, 1.1m, 0.1m, "Spinach", 0.9m, 30m, "g" },
                    { 6, 208m, 0m, 13m, "Salmon", 20m, 100m, "g" },
                    { 7, 112m, 26m, 0.1m, "Sweet Potato", 2.1m, 130m, "g" },
                    { 8, 72m, 0.4m, 4.8m, "Eggs", 6.3m, 50m, "g" },
                    { 9, 150m, 27m, 2.8m, "Oatmeal", 5.3m, 40m, "g" },
                    { 10, 31m, 6m, 0.3m, "Broccoli", 2.5m, 91m, "g" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_Email",
                table: "ApplicationUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSessions_ExerciseId",
                table: "ExerciseSessions",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSessions_WorkoutLogId",
                table: "ExerciseSessions",
                column: "WorkoutLogId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodEntries_FoodItemId",
                table: "FoodEntries",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodEntries_NutritionLogId",
                table: "FoodEntries",
                column: "NutritionLogId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_Name",
                table: "FoodItems",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId",
                table: "Goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionLogs_UserId",
                table: "NutritionLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecords_GoalId",
                table: "ProgressRecords",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId",
                table: "WorkoutLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseSessions");

            migrationBuilder.DropTable(
                name: "FoodEntries");

            migrationBuilder.DropTable(
                name: "ProgressRecords");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutLogs");

            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "NutritionLogs");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");
        }
    }
}
