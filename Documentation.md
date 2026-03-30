# 📚 TÀI LIỆU MÔ TẢ HỆ THỐNG HEALTHSYNC

## Mục Lục
1. [Tổng Quan Project](#1-tổng-quan-project)
2. [Kiến Trúc Clean Architecture](#2-kiến-trúc-clean-architecture)
3. [Cấu Trúc Thư Mục](#3-cấu-trúc-thư-mục)
4. [Domain Layer - Entities & Interfaces](#4-domain-layer---entities--interfaces)
5. [Application Layer - CQRS Pattern](#5-application-layer---cqrs-pattern)
6. [Infrastructure Layer - Repository & Services](#6-infrastructure-layer---repository--services)
7. [Presentation Layer - API Controllers](#7-presentation-layer---api-controllers)
8. [Luồng Nghiệp Vụ Chi Tiết](#8-luồng-nghiệp-vụ-chi-tiết)
9. [Upload Avatar với MinIO](#9-upload-avatar-với-minio)
10. [Docker & Deployment](#10-docker--deployment)
11. [Unit Testing Strategy](#11-unit-testing-strategy)
12. [Các Phương Án Thiết Kế & Trade-offs](#12-các-phương-án-thiết-kế--trade-offs)

---

## 1. Tổng Quan Project

### 1.1. HealthSync là gì?
**HealthSync** là một hệ thống quản lý sức khỏe cá nhân toàn diện (Personal Health Management System), bao gồm:
- **Backend API** (ASP.NET Core 8.0 - Clean Architecture)
- **Web Application** (React + Vite + TailwindCSS)
- **Mobile Application** (Flutter/Dart)
- **AI Chatbot** (Groq AI Integration)

### 1.2. Các Chức Năng Chính
| Module | Mô tả | Endpoint Base |
|--------|-------|---------------|
| **Authentication** | Đăng ký, đăng nhập, OAuth Google | `/api/auth` |
| **User Profile** | Quản lý hồ sơ cá nhân, upload avatar | `/api/userprofile` |
| **Goals** | Tạo và theo dõi mục tiêu sức khỏe | `/api/goals` |
| **Nutrition** | Ghi nhật ký dinh dưỡng | `/api/nutrition` |
| **Workout** | Ghi nhật ký tập luyện | `/api/workout` |
| **AI Chat** | Tư vấn sức khỏe bằng AI | `/api/chat` |
| **Admin** | Quản trị hệ thống | `/api/admin` |

### 1.3. Tech Stack
```
Backend:  ASP.NET Core 8.0 + Entity Framework Core + MediatR (CQRS)
Database: SQL Server 2022
Storage:  MinIO (S3-compatible Object Storage)
Frontend: React 18 + Vite + TailwindCSS + TypeScript
Mobile:   Flutter 3.x + Dart
AI:       Groq API (LLM Integration)
DevOps:   Docker + Docker Compose + Nginx (Reverse Proxy)
```

---

## 2. Kiến Trúc Clean Architecture

### 2.1. Tại Sao Chọn Clean Architecture?

**Vấn đề của kiến trúc truyền thống (N-Tier):**
- Business logic bị "lẫn" vào Controller hoặc Database layer
- Khó thay đổi database hoặc framework
- Unit testing phức tạp vì phụ thuộc nhiều layer

**Clean Architecture giải quyết bằng cách:**
- **Dependency Inversion**: Outer layers phụ thuộc inner layers, không ngược lại
- **Separation of Concerns**: Mỗi layer có trách nhiệm rõ ràng
- **Testability**: Business logic độc lập, dễ test

```
┌─────────────────────────────────────────────────────┐
│                 PRESENTATION LAYER                  │
│    (Controllers, Middleware, API Endpoints)         │
├─────────────────────────────────────────────────────┤
│                INFRASTRUCTURE LAYER                 │
│  (DbContext, Repositories, External Services)       │
├─────────────────────────────────────────────────────┤
│                 APPLICATION LAYER                   │
│    (Commands, Queries, Handlers, DTOs, Services)    │
├─────────────────────────────────────────────────────┤
│                   DOMAIN LAYER                      │
│       (Entities, Interfaces, Business Rules)        │
└─────────────────────────────────────────────────────┘
          ↑ Dependencies flow INWARD only ↑
```

### 2.2. Dependency Rule (Quy Tắc Phụ Thuộc)

```csharp
// ✅ ĐÚNG: Infrastructure phụ thuộc Domain (qua Interface)
public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly HealthSyncDbContext _context;
    // Implementation...
}

// ❌ SAI: Domain KHÔNG ĐƯỢC phụ thuộc Infrastructure
public class ApplicationUser
{
    private readonly DbContext _context; // KHÔNG BAO GIỜ LÀM NHƯ NÀY!
}
```

---

## 3. Cấu Trúc Thư Mục

```
HealthSync/
├── backend/
│   ├── HealthSync.Domain/           # Core business entities & interfaces
│   │   ├── Entities/                # ApplicationUser, Goal, NutritionLog...
│   │   ├── Interfaces/              # IRepository, IService contracts
│   │   └── Constants/               # RoleNames, SystemConstants...
│   │
│   ├── HealthSync.Application/      # Business logic (CQRS)
│   │   ├── Commands/                # Write operations (CreateGoalCommand...)
│   │   ├── Queries/                 # Read operations (GetGoalsQuery...)
│   │   ├── Handlers/                # Command/Query handlers
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   ├── Services/                # Application services
│   │   └── Validators/              # FluentValidation rules
│   │
│   ├── HealthSync.Infrastructure/   # External concerns
│   │   ├── Persistence/             # DbContext, Repositories
│   │   ├── Services/                # Email, Storage, AI services
│   │   ├── Authorization/           # Permission policies
│   │   └── Migrations/              # EF Core migrations
│   │
│   ├── HealthSync.Presentation/     # API layer
│   │   ├── Controllers/             # REST API controllers
│   │   ├── Middleware/              # Custom middleware
│   │   └── Program.cs               # Application bootstrap
│   │
│   └── *Tests/                      # Unit & Integration tests
│
├── HealthSync_web/                  # React Web Application
│   ├── src/
│   │   ├── components/              # Reusable UI components
│   │   ├── pages/                   # Page components (routes)
│   │   ├── services/                # API service calls
│   │   ├── contexts/                # React Context (state management)
│   │   └── hooks/                   # Custom React hooks
│   └── Dockerfile
│
├── HealthSync_mobile/               # Flutter Mobile App
│   ├── lib/
│   │   ├── models/                  # Data models
│   │   ├── screens/                 # UI screens
│   │   ├── services/                # API services
│   │   └── providers/               # State management
│   └── pubspec.yaml
│
├── docker-compose.yml               # Multi-container orchestration
└── nginx/                           # Reverse proxy config
```

---

## 4. Domain Layer - Entities & Interfaces

### 4.1. ApplicationUser Entity

**Vị trí file:** `backend/HealthSync.Domain/Entities/ApplicationUser.cs`

```csharp
public class ApplicationUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }

    // Navigation properties (Eager loading với Include)
    public UserProfile? Profile { get; set; }
    public ICollection<WorkoutLog> WorkoutLogs { get; set; } = new List<WorkoutLog>();
    public ICollection<NutritionLog> NutritionLogs { get; set; } = new List<NutritionLog>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
```

**Tại sao thiết kế như vậy?**

| Quyết định | Lý do |
|------------|-------|
| `PasswordHash` thay vì `Password` | An toàn - không lưu mật khẩu gốc |
| `AvatarUrl` nullable | Cho phép user không có avatar |
| `CreatedAt = DateTime.UtcNow` | Mặc định khi tạo mới, dùng UTC để đồng nhất múi giờ |
| Navigation properties là Collection | Hỗ trợ Eager Loading với `.Include()` |

### 4.2. UserProfile Entity (One-to-One)

**Vị trí file:** `backend/HealthSync.Domain/Entities/UserProfile.cs`

```csharp
public class UserProfile
{
    public int UserId { get; set; }           // PK đồng thời là FK
    public string FullName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public string ActivityLevel { get; set; } = "Moderate";
    public string? AvatarUrl { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;

    // Business logic method
    public bool IsComplete()
    {
        return !string.IsNullOrWhiteSpace(FullName) &&
               !string.IsNullOrWhiteSpace(Gender) &&
               Gender != "Unknown" &&
               HeightCm > 0 &&
               WeightKg > 0 &&
               Dob < DateTime.UtcNow.AddYears(-10);
    }
}
```

**Trường hợp nghiệp vụ:**
- **Tại sao `UserId` vừa là PK vừa là FK?**
  - Đây là mối quan hệ **One-to-One**
  - Mỗi User chỉ có 1 Profile, và mỗi Profile thuộc về đúng 1 User
  - Tiết kiệm 1 column so với việc tạo `ProfileId` riêng

- **Tại sao có method `IsComplete()`?**
  - Business rule: Profile chưa hoàn thành → không được truy cập một số tính năng
  - Logic nằm trong Entity (Domain-Driven Design principle)

### 4.3. Goal Entity

**Vị trí file:** `backend/HealthSync.Domain/Entities/Goal.cs`

```csharp
public class Goal
{
    public int GoalId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;     // "weight_loss", "muscle_gain"
    public decimal TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }                // Nullable - mục tiêu dài hạn
    public string Status { get; set; } = "active";       // active, completed, paused
    public string? Notes { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ProgressRecord> ProgressRecords { get; set; } = new List<ProgressRecord>();
}
```

**Các trạng thái mục tiêu (Status):**
| Status | Mô tả | Được phép cập nhật Progress? |
|--------|-------|------------------------------|
| `active` | Đang thực hiện | ✅ Có |
| `in_progress` | Đang trong tiến trình | ✅ Có |
| `completed` | Đã hoàn thành | ❌ Không |
| `paused` | Tạm dừng | ❌ Không |

### 4.4. Repository Interfaces

**Vị trí file:** `backend/HealthSync.Domain/Interfaces/`

```csharp
// IApplicationUserRepository.cs
public interface IApplicationUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(int id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetAllAsync();
    Task AddAsync(ApplicationUser user);
    Task UpdateAsync(ApplicationUser user);
    Task DeleteAsync(ApplicationUser user);
}

// IUserProfileRepository.cs
public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserProfile>> GetAllAsync();
    Task AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task DeleteAsync(UserProfile profile);
}
```

**Tại sao dùng Interface?**
- **Dependency Inversion**: Application layer chỉ biết Interface, không biết implementation
- **Testability**: Dễ dàng mock trong unit tests
- **Flexibility**: Có thể swap implementation (SQL Server → PostgreSQL) mà không đổi business logic

---

## 5. Application Layer - CQRS Pattern

### 5.1. CQRS là gì?

**CQRS (Command Query Responsibility Segregation)** là pattern tách biệt:
- **Command**: Thay đổi state (Create, Update, Delete) → Không trả về data
- **Query**: Đọc data (Read) → Không thay đổi state

**Trong HealthSync, sử dụng MediatR để implement CQRS:**

```
Request (Command/Query) → MediatR → Handler → Response
```

### 5.2. CreateGoalCommand (Write Operation)

**Vị trí file:** `backend/HealthSync.Application/Commands/CreateGoalCommand.cs`

```csharp
public class CreateGoalCommand : IRequest<GoalResponse>
{
    public int UserId { get; set; }        // Từ JWT Token
    public string Type { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
```

**CreateGoalCommandHandler:**

**Vị trí file:** `backend/HealthSync.Application/Handlers/CreateGoalCommandHandler.cs`

```csharp
public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, GoalResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateGoalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GoalResponse> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        // 1. Tạo entity từ command
        var goal = new Goal
        {
            UserId = request.UserId,
            Type = request.Type,
            TargetValue = request.TargetValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "active",           // Hard-coded mặc định
            Notes = request.Notes
        };

        // 2. Thêm vào DbContext
        _context.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Map sang DTO và return
        return new GoalResponse
        {
            GoalId = goal.GoalId,
            Type = goal.Type,
            TargetValue = goal.TargetValue,
            StartDate = goal.StartDate,
            EndDate = goal.EndDate,
            Status = goal.Status,
            Notes = goal.Notes,
            ProgressRecords = new List<ProgressRecordResponse>()
        };
    }
}
```

**Luồng xử lý:**
```
Controller nhận POST /goals
    ↓
Tạo CreateGoalCommand từ request body
    ↓
MediatR.Send(command)
    ↓
CreateGoalCommandHandler.Handle()
    ↓
Tạo Goal entity → Add to DbContext → SaveChanges
    ↓
Return GoalResponse DTO
    ↓
Controller trả về 201 Created
```

### 5.3. GetGoalsQuery (Read Operation)

**Vị trí file:** `backend/HealthSync.Application/Queries/GetGoalsQuery.cs`

```csharp
public class GetGoalsQuery : IRequest<GetGoalsResponse>
{
    public int UserId { get; set; }
}
```

**GetGoalsQueryHandler:**

**Vị trí file:** `backend/HealthSync.Application/Handlers/GetGoalsQueryHandler.cs`

```csharp
public class GetGoalsQueryHandler : IRequestHandler<GetGoalsQuery, GetGoalsResponse>
{
    private readonly IApplicationDbContext _context;

    public async Task<GetGoalsResponse> Handle(GetGoalsQuery request, CancellationToken cancellationToken)
    {
        // 1. Query với Eager Loading
        var goals = await _context.Goals
            .Where(g => g.UserId == request.UserId)
            .Include(g => g.ProgressRecords)    // Eager load related data
            .ToListAsync(cancellationToken);

        // 2. Map sang DTOs
        var goalResponses = goals.Select(g => new GoalResponse
        {
            GoalId = g.GoalId,
            Type = g.Type,
            // ... mapping
            ProgressRecords = g.ProgressRecords.Select(pr => new ProgressRecordResponse
            {
                // ... mapping
            }).ToList()
        }).ToList();

        return new GetGoalsResponse { Goals = goalResponses };
    }
}
```

### 5.4. AddProgressCommand (Business Logic Phức Tạp)

**Vị trí file:** `backend/HealthSync.Application/Handlers/AddProgressCommandHandler.cs`

```csharp
public async Task<AddProgressResponse> Handle(AddProgressCommand request, CancellationToken cancellationToken)
{
    // 1. KIỂM TRA QUYỀN SỞ HỮU (Authorization)
    var goal = await _context.Goals
        .FirstOrDefaultAsync(g => g.GoalId == request.GoalId && g.UserId == request.UserId, cancellationToken);

    if (goal == null)
    {
        throw new KeyNotFoundException("Goal not found or does not belong to the user.");
    }

    // 2. KIỂM TRA BUSINESS RULE: Chỉ có thể cập nhật tiến độ cho goal đang active
    if (goal.Status != "active" && goal.Status != "in_progress")
    {
        throw new InvalidOperationException(
            $"Chỉ có thể cập nhật tiến độ cho mục tiêu đang hoạt động. Trạng thái hiện tại: {goal.Status}");
    }

    // 3. TẠO PROGRESS RECORD
    var progressRecord = new ProgressRecord
    {
        GoalId = request.GoalId,
        RecordDate = request.RecordDate,
        Value = request.Value,
        Notes = request.Notes,
        WeightKg = request.WeightKg ?? 0,
        WaistCm = request.WaistCm ?? 0
    };

    _context.Add(progressRecord);
    await _context.SaveChangesAsync(cancellationToken);

    return new AddProgressResponse { /* ... */ };
}
```

**Các trường hợp nghiệp vụ được xử lý:**

| Trường hợp | Xử lý | HTTP Response |
|------------|-------|---------------|
| Goal không tồn tại | `KeyNotFoundException` | 404 Not Found |
| Goal thuộc user khác | `KeyNotFoundException` | 404 Not Found |
| Goal đã completed | `InvalidOperationException` | 400 Bad Request |
| Goal đang paused | `InvalidOperationException` | 400 Bad Request |
| Valid request | Tạo ProgressRecord | 200 OK |

---

## 6. Infrastructure Layer - Repository & Services

### 6.1. HealthSyncDbContext

**Vị trí file:** `backend/HealthSync.Infrastructure/Persistence/HealthSyncDbContext.cs`

```csharp
public class HealthSyncDbContext : DbContext, IApplicationDbContext
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<ProgressRecord> ProgressRecords { get; set; }
    public DbSet<WorkoutLog> WorkoutLogs { get; set; }
    public DbSet<NutritionLog> NutritionLogs { get; set; }
    // ... other DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Xóa User → xóa Profile
        });

        // Seed data cho Roles & Permissions
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, RoleName = "Admin", Description = "..." },
            new Role { Id = 2, RoleName = "Customer", Description = "..." }
        );
        
        // ... RolePermissions seed data
    }
}
```

**Tại sao implement IApplicationDbContext?**
- Interface cho phép mock trong unit tests
- Application layer không phụ thuộc trực tiếp vào EF Core

### 6.2. AvatarStorageService (MinIO Integration)

**Vị trí file:** `backend/HealthSync.Infrastructure/Services/AvatarStorageService.cs`

```csharp
public class AvatarStorageService : IAvatarStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _publicUrl;
    private const string AVATAR_BUCKET = "avatars";
    private bool _bucketChecked = false;

    public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string contentType)
    {
        // 1. Đảm bảo bucket tồn tại
        await EnsureAvatarBucketExistsAsync();

        // 2. Tạo unique filename để tránh conflict
        var objectName = $"{Guid.NewGuid()}_{fileName}";

        // 3. Upload lên MinIO
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(AVATAR_BUCKET)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs);

        // 4. Return public URL
        return $"{_publicUrl}/{AVATAR_BUCKET}/{objectName}";
    }

    private async Task EnsureAvatarBucketExistsAsync()
    {
        if (_bucketChecked) return;  // Cache để không check mỗi lần upload

        bool found = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(AVATAR_BUCKET));

        if (!found)
        {
            // Tạo bucket và set public read policy
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(AVATAR_BUCKET));
            
            // Set S3 bucket policy cho phép public read
            var policyJson = @"{
                ""Version"": ""2012-10-17"",
                ""Statement"": [{
                    ""Effect"": ""Allow"",
                    ""Principal"": { ""AWS"": [""*""] },
                    ""Action"": [""s3:GetObject""],
                    ""Resource"": [""arn:aws:s3:::avatars/*""]
                }]
            }";
            
            await _minioClient.SetPolicyAsync(new SetPolicyArgs()
                .WithBucket(AVATAR_BUCKET)
                .WithPolicy(policyJson));
        }
        _bucketChecked = true;
    }
}
```

**Tại sao dùng MinIO thay vì lưu file trực tiếp?**

| Phương án | Ưu điểm | Nhược điểm |
|-----------|---------|------------|
| **Lưu file hệ thống** | Đơn giản, không cần setup | Không scale được, mất file khi container restart |
| **Lưu vào Database (BLOB)** | Đơn giản, backup cùng DB | DB phình to, query chậm |
| **MinIO (S3-compatible)** | Scale tốt, CDN ready, persistent | Cần setup thêm service |

**→ Chọn MinIO vì:**
1. Phù hợp với Docker deployment
2. Volume mount giữ data persistent
3. Tương thích S3 API nên có thể migrate sang AWS S3 sau

---

## 7. Presentation Layer - API Controllers

### 7.1. GoalsController

**Vị trí file:** `backend/HealthSync.Presentation/Controllers/GoalsController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Yêu cầu JWT authentication
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var command = new CreateGoalCommand
        {
            UserId = userId.Value,      // KHÔNG lấy từ request body!
            Type = request.Type,
            TargetValue = request.TargetValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetGoals), new { id = result.GoalId }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetGoals()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var query = new GetGoalsQuery { UserId = userId.Value };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/progress")]
    public async Task<IActionResult> AddProgress(int id, [FromBody] AddProgressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var command = new AddProgressCommand
        {
            GoalId = id,
            UserId = userId.Value,      // Verify ownership trong handler
            // ... other properties
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // Helper method lấy UserId từ JWT Claims
    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            return userId;
        return null;
    }
}
```

**Security Best Practices:**

| Practice | Mô tả |
|----------|-------|
| `[Authorize]` | Yêu cầu Bearer JWT token |
| UserId từ JWT | KHÔNG tin tưởng UserId từ request body |
| Ownership check | Handler verify user sở hữu resource |

### 7.2. UserProfileController - Upload Avatar

**Vị trí file:** `backend/HealthSync.Presentation/Controllers/UserProfileController.cs`

```csharp
[HttpPost("upload-avatar")]
public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request)
{
    var userId = GetUserId();
    if (!userId.HasValue) return Unauthorized();

    // 1. Validate file được upload
    if (request.File == null || request.File.Length == 0)
        return BadRequest("No file uploaded");

    // 2. Validate file type (chỉ cho phép image)
    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
    if (!allowedTypes.Contains(request.File.ContentType.ToLower()))
        return BadRequest("Only image files are allowed");

    // 3. Validate file size (max 5MB)
    if (request.File.Length > 5 * 1024 * 1024)
        return BadRequest("File size must be less than 5MB");

    // 4. Gửi command qua MediatR
    using var stream = request.File.OpenReadStream();
    var command = new UploadAvatarCommand
    {
        UserId = userId.Value,
        FileStream = stream,
        FileName = request.File.FileName,
        ContentType = request.File.ContentType
    };

    var avatarUrl = await _mediator.Send(command);
    return Ok(new { AvatarUrl = avatarUrl });
}
```

---

## 8. Luồng Nghiệp Vụ Chi Tiết

### 8.1. Luồng Tạo Mục Tiêu (Create Goal)

```
┌────────────┐     HTTP POST      ┌────────────────┐
│   Client   │ ─────────────────→ │ GoalsController│
│ (Web/Mobile)│    /api/goals      │  CreateGoal()  │
└────────────┘                    └───────┬────────┘
                                          │
                     ┌────────────────────┴────────────────────┐
                     │ 1. Lấy UserId từ JWT (ClaimTypes.NameId)│
                     │ 2. Validate request body                │
                     │ 3. Tạo CreateGoalCommand                │
                     └────────────────────┬────────────────────┘
                                          │
                                          ▼
                     ┌────────────────────────────────────────┐
                     │              MediatR                   │
                     │    _mediator.Send(command)             │
                     └────────────────────┬───────────────────┘
                                          │
                                          ▼
                     ┌────────────────────────────────────────┐
                     │     CreateGoalCommandHandler           │
                     │ 1. Tạo Goal entity từ command          │
                     │ 2. Set Status = "active" (mặc định)    │
                     │ 3. _context.Add(goal)                  │
                     │ 4. await SaveChangesAsync()            │
                     │ 5. Map sang GoalResponse DTO           │
                     └────────────────────┬───────────────────┘
                                          │
                                          ▼
                     ┌────────────────────────────────────────┐
                     │          SQL Server                    │
                     │   INSERT INTO Goals (...) VALUES (...) │
                     └────────────────────────────────────────┘
```

### 8.2. Luồng Ghi Nhật Ký Dinh Dưỡng

```
┌────────────┐   POST /api/nutrition/food-entry   ┌─────────────────────┐
│   Client   │ ─────────────────────────────────→ │ NutritionController │
└────────────┘                                    └──────────┬──────────┘
                                                             │
                           ┌─────────────────────────────────┴──────────────────────────┐
                           │ 1. GetUserId() từ JWT                                       │
                           │ 2. Tạo AddFoodEntryCommand với:                            │
                           │    - UserId (từ JWT)                                        │
                           │    - LogDate (mặc định = DateTime.UtcNow nếu null)         │
                           │    - FoodItemId, Quantity, MealType (từ request)           │
                           └─────────────────────────────────┬──────────────────────────┘
                                                             │
                                                             ▼
                           ┌─────────────────────────────────────────────────────────────┐
                           │                    AddFoodEntryHandler                     │
                           │ 1. Tìm hoặc tạo NutritionLog cho ngày LogDate              │
                           │ 2. Tìm FoodItem theo FoodItemId                             │
                           │ 3. Tính toán macros:                                        │
                           │    - CaloriesKcal = FoodItem.Calories * Quantity           │
                           │    - ProteinG = FoodItem.Protein * Quantity                │
                           │ 4. Tạo FoodEntry và gắn vào NutritionLog                   │
                           │ 5. Cập nhật TotalCalories của NutritionLog                 │
                           │ 6. SaveChangesAsync()                                       │
                           └─────────────────────────────────────────────────────────────┘
```

### 8.3. Luồng AI Chat (Tích hợp Groq)

```
┌────────────┐   POST /api/chat/message   ┌────────────────┐
│   Client   │ ─────────────────────────→ │ ChatController │
└────────────┘                            └───────┬────────┘
                                                  │
                                                  ▼
                           ┌──────────────────────────────────────────┐
                           │        ChatWithBotQueryHandler          │
                           │                                          │
                           │ 1. Lấy UserProfile từ DB                 │
                           │ 2. Tính BMI, BMR từ profile data         │
                           │ 3. Lấy Active Goals                      │
                           │ 4. Lấy Completed Goals (để khen user)    │
                           │ 5. Lấy 7 ngày NutritionLogs & WorkoutLogs│
                           │ 6. Lấy UserActionLogs (context thao tác) │
                           │ 7. Build JSON context object             │
                           └─────────────────────┬────────────────────┘
                                                 │
                                                 ▼
                           ┌──────────────────────────────────────────┐
                           │           GroqAiChatService              │
                           │                                          │
                           │ 1. BuildSystemPrompt() với full context  │
                           │ 2. POST request to Groq API:             │
                           │    - model: "openai/gpt-oss-120b"        │
                           │    - messages: [system, user]            │
                           │    - max_tokens: 8192                    │
                           │ 3. Parse response                        │
                           └─────────────────────┬────────────────────┘
                                                 │
                                                 ▼
                           ┌──────────────────────────────────────────┐
                           │           Lưu Chat History               │
                           │                                          │
                           │ 1. Lưu user message (role: "user")       │
                           │ 2. Lưu AI response (role: "assistant")   │
                           │ 3. Return AI response cho client         │
                           └──────────────────────────────────────────┘
```

---

## 9. Upload Avatar với MinIO

### 9.1. Luồng Xử Lý Chi Tiết

```
┌────────────┐   POST /api/userprofile/upload-avatar   ┌─────────────────────┐
│   Client   │ ─────────────────────────────────────→  │UserProfileController│
│(form-data) │  [Authorization: Bearer <jwt>]          └──────────┬──────────┘
└────────────┘  [File: avatar.jpg]                                │
                                                                  ▼
                           ┌────────────────────────────────────────────────────────┐
                           │                 Controller Validation                  │
                           │                                                        │
                           │ if (file.Length == 0) return BadRequest("No file")    │
                           │ if (!allowedTypes.Contains(contentType)) return 400   │
                           │ if (file.Length > 5MB) return BadRequest("Too large") │
                           └────────────────────────────┬───────────────────────────┘
                                                        │
                                                        ▼
                           ┌──────────────────────────────────────────────────────┐
                           │                 UploadAvatarHandler                  │
                           │                                                      │
                           │ 1. Gọi _avatarStorageService.UploadAvatarAsync()    │
                           │ 2. Lấy UserProfile từ repository                     │
                           │ 3. Nếu có avatar cũ → Delete từ MinIO               │
                           │ 4. Cập nhật profile.AvatarUrl = newUrl              │
                           │ 5. Cập nhật ApplicationUser.AvatarUrl (đồng bộ)     │
                           │ 6. SaveChangesAsync()                                │
                           └────────────────────────────┬─────────────────────────┘
                                                        │
                                                        ▼
┌──────────────────────────────────────────────────────────────────────────────────┐
│                            AvatarStorageService                                  │
│                                                                                  │
│ 1. EnsureAvatarBucketExistsAsync()  ← Tạo bucket "avatars" nếu chưa có         │
│                                                                                  │
│ 2. Generate unique filename:                                                     │
│    objectName = $"{Guid.NewGuid()}_{originalFileName}"                          │
│    Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890_avatar.jpg"                   │
│                                                                                  │
│ 3. PutObjectAsync() → Upload lên MinIO                                          │
│                                                                                  │
│ 4. Return public URL:                                                            │
│    "http://localhost:9002/avatars/a1b2c3d4..._avatar.jpg"                       │
└──────────────────────────────────────────────────────────────────────────────────┘
```

### 9.2. Tại Sao Cần Đồng Bộ AvatarUrl Giữa 2 Bảng?

```csharp
// Trong UploadAvatarHandler
profile.AvatarUrl = avatarUrl;
await _userProfileRepository.UpdateAsync(profile);

// FIX: Cập nhật luôn ApplicationUser.AvatarUrl để đồng bộ 2 bảng
var user = await _context.ApplicationUsers
    .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);
if (user != null)
{
    user.AvatarUrl = avatarUrl;
    await _context.SaveChangesAsync(cancellationToken);
}
```

**Lý do:**
- Ban đầu, chỉ có `UserProfile.AvatarUrl`
- Sau đó thêm `ApplicationUser.AvatarUrl` để hiển thị trong danh sách users (Admin)
- Cần đồng bộ cả 2 khi upload để tránh data inconsistency

**Phương án thay thế (tương lai):**
- Xóa `ApplicationUser.AvatarUrl`, chỉ dùng `UserProfile.AvatarUrl`
- Khi cần avatar trong list users → JOIN với UserProfile

---

## 10. Docker & Deployment

### 10.1. Docker Compose Architecture

```yaml
# docker-compose.yml
services:
  # 1. DATABASE
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=${SA_PASSWORD}
    volumes:
      - sqlserver-data:/var/opt/mssql  # Persistent storage
    healthcheck:
      test: ["CMD-SHELL", "sqlcmd..."]
    
  # 2. OBJECT STORAGE
  minio:
    image: minio/minio:latest
    command: server /data --console-address ":9001"
    ports:
      - "9002:9000"   # API
      - "9003:9001"   # Console
    volumes:
      - minio-data:/data
      
  # 3. BACKEND API (Scalable)
  backend:
    build: ./backend
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver...
      - MinIO__Endpoint=minio:9000
    depends_on:
      sqlserver: { condition: service_healthy }
      minio: { condition: service_healthy }
    deploy:
      replicas: 2  # Load balancing với 2 instances
      
  # 4. WEB FRONTEND (Scalable)
  web:
    build: ./HealthSync_web
    deploy:
      replicas: 2
      
  # 5. REVERSE PROXY & LOAD BALANCER
  nginx:
    image: nginx:alpine
    ports:
      - "8080:80"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
```

### 10.2. Tại Sao Scale Backend & Web?

```nginx
# nginx.conf
upstream backend_servers {
    server backend:8080;  # Docker tự động load balance giữa 2 replicas
}

upstream web_servers {
    server web:80;
}

server {
    location /api {
        proxy_pass http://backend_servers;
    }
    
    location / {
        proxy_pass http://web_servers;
    }
}
```

**Lợi ích:**
- **High Availability**: 1 instance fail → traffic tự động route sang instance còn lại
- **Performance**: Phân tải request giữa các instances
- **Zero-downtime deployment**: Rolling update từng instance

### 10.3. Backend Dockerfile (Multi-stage Build)

```dockerfile
# Stage 1: BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only csproj files first (layer caching)
COPY *.sln .
COPY HealthSync.Domain/*.csproj HealthSync.Domain/
COPY HealthSync.Application/*.csproj HealthSync.Application/
# ...
RUN dotnet restore

# Copy source and build
COPY . .
RUN dotnet build -c Release -o /app/build

# Stage 2: PUBLISH
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: RUNTIME (minimal image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
USER app  # Security: chạy với non-root user
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthSync.Presentation.dll"]
```

**Tại sao Multi-stage Build?**
| Stage | Image Size | Chứa |
|-------|------------|------|
| SDK (build) | ~700MB | Compiler, SDK |
| ASP.NET (runtime) | ~100MB | Chỉ runtime |

→ Final image chỉ ~100MB thay vì ~700MB

---

## 11. Unit Testing Strategy

### 11.1. Test Structure

```
backend/
├── HealthSync.Domain.Tests/
├── HealthSync.Application.Tests/
│   └── Handlers/
│       ├── CreateGoalCommandHandlerTests.cs
│       ├── AddProgressCommandHandlerTests.cs
│       ├── ChatWithBotQueryHandlerTests.cs
│       └── ...
├── HealthSync.Infrastructure.Tests/
│   └── Services/
│       ├── AvatarStorageServiceTests.cs
│       ├── GroqAiChatServiceTests.cs
│       └── ...
└── HealthSync.Presentation.Tests/
    └── Controllers/
```

### 11.2. Testing CreateGoalCommandHandler

**Vị trí file:** `backend/HealthSync.Application.Tests/Handlers/CreateGoalCommandHandlerTests.cs`

```csharp
public class CreateGoalCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateGoalCommandHandler _handler;

    public CreateGoalCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateGoalCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidGoal_CreatesSuccessfully()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "weight_loss",
            TargetValue = 65.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(3),
            Notes = "Lose 5kg for summer"
        };

        Goal? capturedGoal = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { if (capturedGoal != null) capturedGoal.GoalId = 42; })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.GoalId);
        Assert.Equal("weight_loss", result.Type);
        Assert.Equal("active", result.Status);  // Default status
        Assert.Empty(result.ProgressRecords);   // New goal has no progress
    }

    [Fact]
    public async Task Handle_NewGoal_SetsStatusToActive()
    {
        // Arrange
        var command = new CreateGoalCommand { /* ... */ };
        
        Goal? capturedGoal = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedGoal);
        Assert.Equal("active", capturedGoal.Status);  // Business rule verified
    }
}
```

### 11.3. Test Coverage Focus

| Layer | Priority | Lý do |
|-------|----------|-------|
| **Application Handlers** | ⭐⭐⭐ Cao nhất | Chứa business logic quan trọng |
| **Infrastructure Services** | ⭐⭐ Cao | External integration (AI, Storage) |
| **Domain Entities** | ⭐ Thấp | Chỉ có properties, ít logic |
| **Controllers** | ⭐ Thấp | Chỉ là "thin wrapper", logic ở Handler |

---

## 12. Các Phương Án Thiết Kế & Trade-offs

### 12.1. Repository Pattern vs DbContext Trực Tiếp

**Phương án 1: Repository Pattern**
```csharp
// Interface
public interface IApplicationUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(int id);
    // ...
}

// Implementation
public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly HealthSyncDbContext _context;
    public async Task<ApplicationUser?> GetByIdAsync(int id)
        => await _context.ApplicationUsers.FindAsync(id);
}
```

**Phương án 2: IApplicationDbContext trực tiếp**
```csharp
// Interface expose IQueryable
public interface IApplicationDbContext
{
    IQueryable<ApplicationUser> ApplicationUsers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

// Usage in Handler
var user = await _context.ApplicationUsers
    .Where(u => u.UserId == id)
    .Include(u => u.Profile)
    .FirstOrDefaultAsync();
```

**HealthSync chọn: Hybrid Approach**
- Dùng `IApplicationDbContext` cho hầu hết CRUD operations
- Dùng Repository cho logic phức tạp (GetByEmail with validation, etc.)

| Tiêu chí | Repository | DbContext trực tiếp |
|----------|------------|---------------------|
| Flexibility | Hạn chế (fixed methods) | Cao (full LINQ) |
| Testability | Dễ mock | Cần MockDbSet |
| Code repetition | Cao (nhiều method giống nhau) | Thấp |
| Learning curve | Cao | Thấp |

### 12.2. AvatarUrl: 1 Bảng hay 2 Bảng?

**Vấn đề hiện tại:**
```
ApplicationUser.AvatarUrl  ←──┐
                              │ Cần đồng bộ!
UserProfile.AvatarUrl     ←──┘
```

**Phương án 1: Chỉ lưu ở UserProfile (Recommended)**
```csharp
// Khi cần avatar trong list users
var users = await _context.ApplicationUsers
    .Include(u => u.Profile)
    .Select(u => new UserDto {
        AvatarUrl = u.Profile != null ? u.Profile.AvatarUrl : null
    });
```

**Phương án 2: Chỉ lưu ở ApplicationUser**
- Profile không có avatar riêng

**Phương án 3: Giữ cả 2 + đồng bộ (Current)**
- Pros: Không cần JOIN khi list users
- Cons: Data duplication, risk inconsistency

### 12.3. Authentication: JWT vs Session

| Tiêu chí | JWT (Current) | Session |
|----------|--------------|---------|
| **Stateless** | ✅ Không cần lưu server | ❌ Cần Session Store |
| **Scale** | ✅ Dễ scale (bất kỳ server nào verify được) | ❌ Cần sticky session/Redis |
| **Mobile friendly** | ✅ Lưu token trên device | ❌ Cookie phức tạp |
| **Token size** | ❌ Lớn hơn session ID | ✅ Nhỏ |
| **Revocation** | ❌ Khó revoke (cần blacklist) | ✅ Xóa session = logout ngay |

**→ JWT phù hợp với HealthSync vì:**
- Có Mobile app (Flutter)
- Microservices-ready
- Stateless API design

### 12.4. AI Context: Gửi Full History vs Summarize

**Phương án 1: Gửi full 7 ngày data (Current)**
```csharp
var recentLogs = await _context.NutritionLogs
    .Where(n => n.UserId == userId && n.LogDate >= DateTime.UtcNow.AddDays(-7))
    .Include(n => n.FoodEntries)
    .ToListAsync();
```
- Pros: AI có đủ context để cá nhân hóa
- Cons: Token usage cao, có thể exceed limit

**Phương án 2: Summarize trước khi gửi**
```csharp
var summary = new {
    AvgDailyCalories = recentLogs.Average(l => l.TotalCalories),
    WorkoutDaysPerWeek = workoutLogs.Count(),
    // ...
};
```
- Pros: Tiết kiệm tokens
- Cons: Mất chi tiết (AI không biết user ăn gì cụ thể)

**Phương án 3: Hybrid (Recommended for future)**
- Gửi summary + 2-3 ngày gần nhất chi tiết

---

## Kết Luận

Hệ thống HealthSync được thiết kế với các nguyên tắc:

1. **Clean Architecture**: Code dễ maintain, test, và evolve
2. **CQRS với MediatR**: Tách biệt read/write, loose coupling
3. **Docker-first**: Consistent environment từ dev đến production
4. **Security by design**: JWT auth, UserId từ token, file validation
5. **AI-powered**: Cá nhân hóa trải nghiệm với context-aware chatbot

**Các cải tiến tiềm năng:**
- [ ] Implement CQRS read/write database separation
- [ ] Add Redis caching cho frequently accessed data
- [ ] Migrate avatar storage sang managed S3 cho Production
- [ ] Implement Event Sourcing cho audit logs
- [ ] Add GraphQL API cho flexible querying

---

*Tài liệu được tạo ngày: 07/01/2026*
*Phiên bản: 1.0*
*Tác giả: HealthSync Development Team*
