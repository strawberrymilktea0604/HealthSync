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
                    { 101, "User", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9085), "Xem danh sách người dùng", "USER_READ" },
                    { 102, "User", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9091), "Khóa tài khoản người dùng", "USER_BAN" },
                    { 103, "User", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9093), "Cập nhật vai trò người dùng", "USER_UPDATE_ROLE" },
                    { 104, "User", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9094), "Xóa người dùng", "USER_DELETE" },
                    { 201, "Exercise", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9095), "Xem thư viện bài tập", "EXERCISE_READ" },
                    { 202, "Exercise", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9096), "Thêm bài tập mới", "EXERCISE_CREATE" },
                    { 203, "Exercise", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9098), "Cập nhật bài tập", "EXERCISE_UPDATE" },
                    { 204, "Exercise", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9099), "Xóa bài tập", "EXERCISE_DELETE" },
                    { 301, "Food", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9100), "Xem thư viện thực phẩm", "FOOD_READ" },
                    { 302, "Food", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9101), "Thêm thực phẩm mới", "FOOD_CREATE" },
                    { 303, "Food", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9102), "Cập nhật thực phẩm", "FOOD_UPDATE" },
                    { 304, "Food", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9104), "Xóa thực phẩm", "FOOD_DELETE" },
                    { 401, "WorkoutLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9106), "Xem nhật ký tập luyện", "WORKOUT_LOG_READ" },
                    { 402, "WorkoutLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9107), "Tạo nhật ký tập luyện", "WORKOUT_LOG_CREATE" },
                    { 403, "WorkoutLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9108), "Cập nhật nhật ký tập luyện", "WORKOUT_LOG_UPDATE" },
                    { 404, "WorkoutLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9109), "Xóa nhật ký tập luyện", "WORKOUT_LOG_DELETE" },
                    { 501, "NutritionLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9110), "Xem nhật ký dinh dưỡng", "NUTRITION_LOG_READ" },
                    { 502, "NutritionLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9112), "Tạo nhật ký dinh dưỡng", "NUTRITION_LOG_CREATE" },
                    { 503, "NutritionLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9113), "Cập nhật nhật ký dinh dưỡng", "NUTRITION_LOG_UPDATE" },
                    { 504, "NutritionLog", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9114), "Xóa nhật ký dinh dưỡng", "NUTRITION_LOG_DELETE" },
                    { 601, "Goal", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9115), "Xem mục tiêu", "GOAL_READ" },
                    { 602, "Goal", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9116), "Tạo mục tiêu", "GOAL_CREATE" },
                    { 603, "Goal", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9117), "Cập nhật mục tiêu", "GOAL_UPDATE" },
                    { 604, "Goal", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9118), "Xóa mục tiêu", "GOAL_DELETE" },
                    { 701, "Dashboard", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9119), "Xem dashboard cá nhân", "DASHBOARD_VIEW" },
                    { 702, "Dashboard", new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9120), "Xem dashboard admin", "DASHBOARD_ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(8866), "Quản trị viên hệ thống, có toàn quyền", "Admin" },
                    { 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(8872), "Người dùng cuối sử dụng app", "Customer" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "GrantedAt" },
                values: new object[,]
                {
                    { 101, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9174) },
                    { 102, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9177) },
                    { 103, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9178) },
                    { 104, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9178) },
                    { 201, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9179) },
                    { 202, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9180) },
                    { 203, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9181) },
                    { 204, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9182) },
                    { 301, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9182) },
                    { 302, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9183) },
                    { 303, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9184) },
                    { 304, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9185) },
                    { 401, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9186) },
                    { 402, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9186) },
                    { 403, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9188) },
                    { 404, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9189) },
                    { 501, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9190) },
                    { 502, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9191) },
                    { 503, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9191) },
                    { 504, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9192) },
                    { 601, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9193) },
                    { 602, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9194) },
                    { 603, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9194) },
                    { 604, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9195) },
                    { 701, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9196) },
                    { 702, 1, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9196) },
                    { 201, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9197) },
                    { 301, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9198) },
                    { 401, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9199) },
                    { 402, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9199) },
                    { 403, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9200) },
                    { 404, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9201) },
                    { 501, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9202) },
                    { 502, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9202) },
                    { 503, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9203) },
                    { 504, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9204) },
                    { 601, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9204) },
                    { 602, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9205) },
                    { 603, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9206) },
                    { 604, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9207) },
                    { 701, 2, new DateTime(2026, 1, 5, 17, 45, 55, 191, DateTimeKind.Utc).AddTicks(9207) }
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
