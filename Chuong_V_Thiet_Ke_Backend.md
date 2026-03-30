# CHƯƠNG V: THIẾT KẾ BACKEND

## 5.1. Tổng quan kiến trúc Backend

### 5.1.1. Mô hình kiến trúc
Backend của HealthSync được thiết kế theo kiến trúc Clean Architecture với 4 lớp chính:
- Domain Layer: Chứa các entity và business logic cốt lõi
- Application Layer: Xử lý các use case và business logic
- Infrastructure Layer: Triển khai các dịch vụ hạ tầng
- Presentation Layer: Xử lý HTTP requests và responses

### 5.1.2. Công nghệ sử dụng
- Framework: ASP.NET Core 8.0
- Database: SQL Server
- ORM: Entity Framework Core
- Pattern: CQRS với MediatR
- Authentication: JWT Bearer Token
- Storage: MinIO
- AI Service: Groq API
- Documentation: Swagger/OpenAPI

### 5.1.3. Cấu trúc Solution
```
HealthSync.Backend.sln
├── HealthSync.Domain
├── HealthSync.Application
├── HealthSync.Infrastructure
├── HealthSync.Presentation
├── HealthSync.Domain.Tests
├── HealthSync.Application.Tests
├── HealthSync.Infrastructure.Tests
├── HealthSync.Presentation.Tests
└── HealthSync.IntegrationTests
```

## 5.2. Domain Layer

### 5.2.1. Entities
Các entity chính trong hệ thống:

**5.2.1.1. ApplicationUser**
- Thông tin tài khoản người dùng
- Liên kết với UserProfile, UserRole

**5.2.1.2. UserProfile**
- Thông tin cá nhân người dùng (tên, tuổi, chiều cao, cân nặng)
- Thông tin sức khỏe và mục tiêu

**5.2.1.3. Goal**
- Mục tiêu của người dùng (giảm cân, tăng cân, tập luyện)
- Theo dõi tiến độ đạt mục tiêu

**5.2.1.4. WorkoutLog và ExerciseSession**
- Ghi nhận hoạt động tập luyện
- Chi tiết bài tập trong từng buổi tập

**5.2.1.5. Exercise**
- Thông tin bài tập (tên, nhóm cơ, độ khó)
- Hướng dẫn thực hiện

**5.2.1.6. NutritionLog, FoodEntry, FoodItem**
- Ghi nhận chế độ dinh dưỡng hàng ngày
- Thông tin chi tiết món ăn và calories

**5.2.1.7. Role, Permission, RolePermission, UserRole**
- Quản lý phân quyền người dùng
- Kiểm soát truy cập tài nguyên

**5.2.1.8. ChatMessage**
- Lưu trữ lịch sử chat với AI HealthBot

**5.2.1.9. UserActionLog**
- Ghi nhận các hành động của người dùng trong hệ thống

### 5.2.2. Constants
- PermissionCodes: Các mã quyền hạn trong hệ thống
- RoleNames: Tên các vai trò (Admin, Customer)

### 5.2.3. Interfaces
- IApplicationDbContext: Interface cho database context
- IApplicationUserRepository: Repository cho ApplicationUser
- IUserProfileRepository: Repository cho UserProfile

## 5.3. Application Layer

### 5.3.1. Commands (CQRS Pattern)
Các command xử lý thay đổi dữ liệu:
- RegisterUserCommand: Đăng ký người dùng mới
- RegisterAdminCommand: Đăng ký admin
- LoginCommand: Đăng nhập
- CreateGoalCommand: Tạo mục tiêu mới
- AddWorkoutLogCommand: Thêm nhật ký tập luyện
- AddFoodEntryCommand: Thêm món ăn vào nhật ký dinh dưỡng
- UpdateUserProfileCommand: Cập nhật thông tin người dùng
- UpdateUserRoleCommand: Cập nhật vai trò người dùng

### 5.3.2. Queries (CQRS Pattern)
Các query xử lý truy vấn dữ liệu:
- GetUserProfileQuery: Lấy thông tin người dùng
- GetExercisesQuery: Lấy danh sách bài tập
- GetWorkoutLogsQuery: Lấy lịch sử tập luyện
- GetFoodItemsQuery: Lấy danh sách món ăn
- GetNutritionLogByDateQuery: Lấy nhật ký dinh dưỡng theo ngày
- GetGoalsQuery: Lấy danh sách mục tiêu
- GetAdminStatisticsQuery: Lấy thống kê hệ thống (admin)
- ChatWithBotQuery: Chat với AI HealthBot

### 5.3.3. Handlers
Xử lý logic cho Commands và Queries:
- RegisterUserCommandHandler
- LoginCommandHandler
- WorkoutCommandHandlers
- NutritionCommandHandlers
- GoalCommandHandlers
- ChatQueryHandler

### 5.3.4. DTOs (Data Transfer Objects)
Các DTO truyền dữ liệu giữa layers:
- AuthResponseDto
- UserProfileDto
- WorkoutLogDto
- ExerciseDto
- NutritionLogDto
- GoalDto
- ChatRequestDto, ChatResponseDto
- AdminStatisticsDto

### 5.3.5. Validators
Validation sử dụng FluentValidation:
- RegisterUserCommandValidator
- LoginCommandValidator
- CreateGoalCommandValidator
- AddWorkoutLogValidator
- AddFoodEntryValidator

### 5.3.6. Services
- IApplicationUserService: Quản lý người dùng
- IJwtTokenService: Tạo và xác thực JWT token
- IOtpService: Quản lý OTP
- IAiChatService: Tích hợp AI chatbot

### 5.3.7. Behaviors
- AuditLogBehavior: Tự động ghi log các Command

## 5.4. Infrastructure Layer

### 5.4.1. Persistence
**5.4.1.1. HealthSyncDbContext**
- DbContext chính của ứng dụng
- Định nghĩa DbSet cho tất cả entities
- Cấu hình relationships và constraints

**5.4.1.2. Repositories**
- ApplicationUserRepository
- UserProfileRepository
- Triển khai các interface từ Domain Layer

**5.4.1.3. DataSeeder**
- Khởi tạo dữ liệu mẫu
- Tạo roles, permissions mặc định
- Seed exercises và food items

**5.4.1.4. Migrations**
- Quản lý schema database
- Version control cho database

### 5.4.2. Services
**5.4.2.1. AuthService**
- Xử lý đăng ký, đăng nhập
- Quản lý session

**5.4.2.2. JwtTokenService**
- Tạo JWT token
- Validate token

**5.4.2.3. EmailService**
- Gửi email xác thực
- Gửi email reset password

**5.4.2.4. GoogleAuthService**
- OAuth2 với Google
- Đăng nhập qua Google

**5.4.2.5. GroqAiChatService**
- Tích hợp Groq AI API
- Xử lý chat với HealthBot

**5.4.2.6. MinioStorageService**
- Upload/download files
- Quản lý avatar và images

**5.4.2.7. AvatarStorageService**
- Chuyên biệt cho avatar
- Resize và optimize images

**5.4.2.8. CurrentUserService**
- Lấy thông tin user hiện tại từ HttpContext

### 5.4.3. Authorization
- RequirePermissionAttribute: Custom authorization attribute
- PermissionAuthorizationHandler: Xử lý authorization logic

## 5.5. Presentation Layer

### 5.5.1. Program.cs
Cấu hình chính của ứng dụng:
- Đăng ký services (DI)
- Cấu hình CORS
- Cấu hình JWT Authentication
- Cấu hình Swagger/OpenAPI
- Setup middleware pipeline
- Migration tự động với retry policy

### 5.5.2. Controllers
**5.5.2.1. AuthController** (/api/auth)
- POST /register: Đăng ký user
- POST /register-admin: Đăng ký admin
- POST /login: Đăng nhập
- POST /send-verification-code: Gửi mã xác thực
- POST /verify-code: Xác thực mã OTP
- GET /google/web: OAuth Google web
- GET /google/callback: Callback OAuth
- POST /google/mobile: OAuth Google mobile
- POST /forgot-password: Quên mật khẩu
- POST /reset-password: Đặt lại mật khẩu
- POST /verify-reset-otp: Xác thực OTP reset
- POST /resend-reset-otp: Gửi lại OTP
- POST /set-password: Đặt mật khẩu cho Google user

**5.5.2.2. UserProfileController** (/api/UserProfile)
- GET /: Lấy thông tin profile
- PUT /: Cập nhật profile
- POST /upload-avatar: Upload avatar

**5.5.2.3. WorkoutController** (/api/Workout)
- GET /exercises: Lấy danh sách bài tập (có filter)
- GET /workout-logs: Lấy nhật ký tập luyện
- POST /workout-logs: Thêm nhật ký tập luyện
- DELETE /workout-logs/{id}: Xóa nhật ký

**5.5.2.4. NutritionController** (/api/Nutrition)
- GET /food-items: Lấy danh sách món ăn
- GET /nutrition-log: Lấy nhật ký dinh dưỡng theo ngày
- GET /nutrition-logs: Lấy nhật ký theo khoảng thời gian
- POST /food-entry: Thêm món ăn
- DELETE /food-entry/{id}: Xóa món ăn
- POST /nutrition-logs: Tạo nhật ký dinh dưỡng

**5.5.2.5. GoalsController** (/api/Goals)
- POST /: Tạo mục tiêu mới
- GET /: Lấy danh sách mục tiêu
- POST /{id}/progress: Cập nhật tiến độ

**5.5.2.6. ChatController** (/api/Chat)
- POST /ask: Chat với AI HealthBot
- GET /history: Lấy lịch sử chat
- GET /health: Health check

**5.5.2.7. DashboardController** (/api/Dashboard)
- GET /summary: Tổng quan dashboard
- GET /customer: Dashboard cho customer

**5.5.2.8. AdminController** (/api/Admin)
- GET /dashboard: Dashboard admin
- GET /users: Danh sách người dùng
- GET /users/{userId}: Chi tiết người dùng
- PUT /users/{userId}/role: Cập nhật role
- POST /users: Tạo người dùng mới
- PUT /users/{userId}: Cập nhật thông tin
- PUT /users/{userId}/avatar: Cập nhật avatar
- DELETE /users/{userId}: Xóa người dùng
- PUT /users/{userId}/password: Đổi mật khẩu
- PUT /users/{userId}/status: Cập nhật trạng thái

**5.5.2.9. AdminStatisticsController** (/api/admin)
- GET /statistics: Thống kê tổng quan
- GET /statistics/users: Thống kê người dùng
- GET /statistics/workouts: Thống kê tập luyện
- GET /statistics/nutrition: Thống kê dinh dưỡng
- GET /statistics/goals: Thống kê mục tiêu

**5.5.2.10. ExercisesController** (/api/admin/Exercises)
- GET /: Danh sách bài tập (admin)
- GET /{id}: Chi tiết bài tập
- POST /: Tạo bài tập mới
- PUT /{id}: Cập nhật bài tập
- DELETE /{id}: Xóa bài tập
- PUT /{id}/image: Upload ảnh bài tập

**5.5.2.11. FoodItemsController** (/api/admin/FoodItems)
- GET /: Danh sách món ăn (admin)
- GET /{id}: Chi tiết món ăn
- POST /: Tạo món ăn mới
- PUT /{id}: Cập nhật món ăn
- DELETE /{id}: Xóa món ăn
- PUT /{id}/image: Upload ảnh món ăn

**5.5.2.12. UserManagementController** (/api/admin/UserManagement)
- POST /{userId}/roles/{roleId}: Thêm role cho user
- DELETE /{userId}/roles/{roleId}: Xóa role
- GET /{userId}/roles: Lấy danh sách roles
- GET /{userId}/permissions: Lấy danh sách permissions

### 5.5.3. Middleware
**5.5.3.1. GlobalExceptionHandler**
- Xử lý exception toàn cục
- Trả về response lỗi chuẩn
- Log các exception

### 5.5.4. Configuration Files
- appsettings.json: Cấu hình chung
- appsettings.Development.json: Cấu hình môi trường dev
- appsettings.Production.json: Cấu hình môi trường production

## 5.6. Testing

### 5.6.1. Unit Tests
- HealthSync.Domain.Tests: Test entities
- HealthSync.Application.Tests: Test handlers, validators
- HealthSync.Infrastructure.Tests: Test repositories, services
- HealthSync.Presentation.Tests: Test controllers

### 5.6.2. Integration Tests
- HealthSync.IntegrationTests: Test end-to-end scenarios
- CustomWebApplicationFactory: Test server setup
- SecurityIntegrationTests: Test authentication/authorization

### 5.6.3. Coverage
Sử dụng file HealthSync.runsettings để cấu hình code coverage

## 5.7. Công cụ hỗ trợ

### 5.7.1. Development Tools
- Visual Studio 2022 / VS Code
- SQL Server Management Studio
- Postman / Swagger UI
- Docker Desktop

### 5.7.2. Libraries và Packages
- MediatR: CQRS pattern
- FluentValidation: Validation
- AutoMapper: Object mapping
- Entity Framework Core: ORM
- Swashbuckle: Swagger/OpenAPI
- Polly: Retry policies
- MinIO SDK: Object storage
- Newtonsoft.Json: JSON serialization
- xUnit: Unit testing
- Moq: Mocking framework

### 5.7.3. DevOps Tools
- Docker: Containerization
- Docker Compose: Multi-container orchestration
- Git: Version control
- GitHub Actions: CI/CD

### 5.7.4. External Services
- SQL Server: Database
- MinIO: Object storage
- Groq API: AI chatbot
- Google OAuth2: Authentication
- SMTP Server: Email service

## 5.8. Bảo mật

### 5.8.1. Authentication
- JWT Bearer Token
- Google OAuth2
- OTP verification
- Password hashing với bcrypt

### 5.8.2. Authorization
- Role-based access control (RBAC)
- Permission-based authorization
- Custom authorization handlers

### 5.8.3. Data Protection
- SQL injection prevention với EF Core
- XSS protection
- CORS policy
- HTTPS enforcement

### 5.8.4. Logging và Monitoring
- AuditLogBehavior: Ghi log tất cả commands
- UserActionLog: Theo dõi hành động người dùng
- ILogger: Application logging

## 5.9. Deployment

### 5.9.1. Docker Configuration
Dockerfile cho backend:
- Base image: ASP.NET Core Runtime
- Multi-stage build
- Tối ưu layer caching

### 5.9.2. Environment Variables
- JWT_SECRET_KEY: Khóa bí mật JWT
- GROQ_API_KEY: API key cho Groq
- ConnectionStrings__DefaultConnection: Connection string database
- MinIO__Endpoint, MinIO__AccessKey, MinIO__SecretKey
- FRONTEND_URL: URL của frontend

### 5.9.3. Health Checks
- /health endpoint: Kiểm tra API hoạt động
- Database connectivity check
- Retry policy cho migration

### 5.9.4. Scalability
- Stateless design
- Database connection pooling
- Caching strategies
- Load balancing support

---

**Kết luận Chương V:**

Backend của HealthSync được thiết kế theo các best practices hiện đại với Clean Architecture, CQRS pattern, và dependency injection. Kiến trúc này đảm bảo code dễ bảo trì, dễ test, và dễ mở rộng. Hệ thống cung cấp đầy đủ các API endpoints phục vụ cho cả người dùng thông thường và quản trị viên, đồng thời tích hợp các dịch vụ bên ngoài như AI chatbot và OAuth2. Việc áp dụng các công cụ testing và DevOps tools giúp đảm bảo chất lượng code và tự động hóa quy trình phát triển.
