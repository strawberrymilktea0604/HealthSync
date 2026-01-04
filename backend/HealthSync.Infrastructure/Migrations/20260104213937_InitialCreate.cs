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
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    FatG = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.FoodItemId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    ChatMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContextData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.ChatMessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "UserActionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetaDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActionLogs_ApplicationUsers_UserId",
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
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
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
                    MealType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                table: "Permissions",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "PermissionCode" },
                values: new object[,]
                {
                    { 101, "User", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5199), "Xem danh sách người dùng", "USER_READ" },
                    { 102, "User", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5208), "Khóa tài khoản người dùng", "USER_BAN" },
                    { 103, "User", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5209), "Cập nhật vai trò người dùng", "USER_UPDATE_ROLE" },
                    { 104, "User", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5211), "Xóa người dùng", "USER_DELETE" },
                    { 201, "Exercise", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5212), "Xem thư viện bài tập", "EXERCISE_READ" },
                    { 202, "Exercise", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5214), "Thêm bài tập mới", "EXERCISE_CREATE" },
                    { 203, "Exercise", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5215), "Cập nhật bài tập", "EXERCISE_UPDATE" },
                    { 204, "Exercise", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5217), "Xóa bài tập", "EXERCISE_DELETE" },
                    { 301, "Food", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5218), "Xem thư viện thực phẩm", "FOOD_READ" },
                    { 302, "Food", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5220), "Thêm thực phẩm mới", "FOOD_CREATE" },
                    { 303, "Food", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5221), "Cập nhật thực phẩm", "FOOD_UPDATE" },
                    { 304, "Food", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5223), "Xóa thực phẩm", "FOOD_DELETE" },
                    { 401, "WorkoutLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5225), "Xem nhật ký tập luyện", "WORKOUT_LOG_READ" },
                    { 402, "WorkoutLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5226), "Tạo nhật ký tập luyện", "WORKOUT_LOG_CREATE" },
                    { 403, "WorkoutLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5227), "Cập nhật nhật ký tập luyện", "WORKOUT_LOG_UPDATE" },
                    { 404, "WorkoutLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5228), "Xóa nhật ký tập luyện", "WORKOUT_LOG_DELETE" },
                    { 501, "NutritionLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5230), "Xem nhật ký dinh dưỡng", "NUTRITION_LOG_READ" },
                    { 502, "NutritionLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5231), "Tạo nhật ký dinh dưỡng", "NUTRITION_LOG_CREATE" },
                    { 503, "NutritionLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5232), "Cập nhật nhật ký dinh dưỡng", "NUTRITION_LOG_UPDATE" },
                    { 504, "NutritionLog", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5234), "Xóa nhật ký dinh dưỡng", "NUTRITION_LOG_DELETE" },
                    { 601, "Goal", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5235), "Xem mục tiêu", "GOAL_READ" },
                    { 602, "Goal", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5293), "Tạo mục tiêu", "GOAL_CREATE" },
                    { 603, "Goal", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5294), "Cập nhật mục tiêu", "GOAL_UPDATE" },
                    { 604, "Goal", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5295), "Xóa mục tiêu", "GOAL_DELETE" },
                    { 701, "Dashboard", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5297), "Xem dashboard cá nhân", "DASHBOARD_VIEW" },
                    { 702, "Dashboard", new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5298), "Xem dashboard admin", "DASHBOARD_ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(4778), "Quản trị viên hệ thống, có toàn quyền", "Admin" },
                    { 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(4792), "Người dùng cuối sử dụng app", "Customer" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAt" },
                values: new object[,]
                {
                    { 101, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5354) },
                    { 102, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5357) },
                    { 103, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5358) },
                    { 104, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5359) },
                    { 201, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5360) },
                    { 202, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5361) },
                    { 203, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5362) },
                    { 204, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5363) },
                    { 301, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5364) },
                    { 302, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5364) },
                    { 303, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5365) },
                    { 304, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5366) },
                    { 401, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5367) },
                    { 402, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5368) },
                    { 403, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5370) },
                    { 404, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5370) },
                    { 501, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5371) },
                    { 502, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5371) },
                    { 503, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5372) },
                    { 504, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5373) },
                    { 601, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5373) },
                    { 602, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5374) },
                    { 603, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5375) },
                    { 604, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5376) },
                    { 701, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5377) },
                    { 702, 1, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5378) },
                    { 201, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5379) },
                    { 301, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5379) },
                    { 401, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5380) },
                    { 402, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5381) },
                    { 403, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5382) },
                    { 404, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5383) },
                    { 501, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5383) },
                    { 502, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5384) },
                    { 503, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5385) },
                    { 504, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5385) },
                    { 601, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5386) },
                    { 602, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5388) },
                    { 603, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5389) },
                    { 604, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5389) },
                    { 701, 2, new DateTime(2026, 1, 4, 21, 39, 36, 439, DateTimeKind.Utc).AddTicks(5390) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_Email",
                table: "ApplicationUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId_CreatedAt",
                table: "ChatMessages",
                columns: new[] { "UserId", "CreatedAt" });

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
                name: "IX_Permissions_PermissionCode",
                table: "Permissions",
                column: "PermissionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecords_GoalId",
                table: "ProgressRecords",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserActionLogs_UserId_Timestamp",
                table: "UserActionLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId",
                table: "WorkoutLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ExerciseSessions");

            migrationBuilder.DropTable(
                name: "FoodEntries");

            migrationBuilder.DropTable(
                name: "ProgressRecords");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserActionLogs");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "UserRoles");

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
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");
        }
    }
}
