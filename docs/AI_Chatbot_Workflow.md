# 🤖 Tài liệu Luồng Hoạt động AI Chatbot - HealthSync

> **Last Updated:** January 5, 2026  
> **Version:** 3.0  
> **AI Model:** Groq AI (openai/gpt-oss-120b)

---

## 📋 Mục lục

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [Kiến trúc tổng thể](#2-kiến-trúc-tổng-thể)
3. [Chi tiết luồng hoạt động](#3-chi-tiết-luồng-hoạt-động)
4. [Context Data & AI Prompt Engineering](#4-context-data--ai-prompt-engineering)
5. [Components và Implementation](#5-components-và-implementation)
6. [Database Schema](#6-database-schema)
7. [API Endpoints](#7-api-endpoints)
8. [Frontend Integration](#8-frontend-integration)
9. [Security & Authorization](#9-security--authorization)
10. [Error Handling & Best Practices](#10-error-handling--best-practices)

---

## 1. Tổng quan hệ thống

### 🎯 Giới thiệu

**HealthSync AI Chatbot** là trợ lý sức khỏe thông minh, sử dụng **Groq AI (openai/gpt-oss-120b)** để cung cấp tư vấn dinh dưỡng và luyện tập **cá nhân hóa 100%** dựa trên:
- ✅ Dữ liệu sức khỏe thực tế của người dùng (Profile, BMI, BMR)
- ✅ Mục tiêu hiện tại (giảm cân, tăng cân, duy trì)
- ✅ Lịch sử dinh dưỡng & tập luyện (7 ngày gần nhất)
- ✅ **Data Warehouse Lite**: Theo dõi 50 thao tác gần nhất (ăn gì, tập gì, khi nào)

### ✨ Tính năng chính

| Tính năng | Mô tả | Công nghệ |
|-----------|-------|-----------|
| **Tư vấn cá nhân hóa** | AI phân tích dữ liệu thực của user để đưa lời khuyên cụ thể | Groq AI (openai/gpt-oss-120b) |
| **Context Awareness** | Nhớ toàn bộ hồ sơ sức khỏe + hoạt động 7 ngày qua | JSON Context Snapshot |
| **Data Warehouse** | Theo dõi 50 thao tác gần nhất (ăn, tập, view reports) | UserActionLogs Entity |
| **Chat History** | Lưu trữ vĩnh viễn cuộc hội thoại user-AI | ChatMessage Entity + GUID |
| **Real-time Response** | Trả lời tức thì, emoji phong phú | Async/Await + Groq API |
| **Security** | JWT Authentication + Role-based | ASP.NET Core Identity |

### 🛠️ Tech Stack

- **Backend:** ASP.NET Core 8.0, MediatR CQRS, Entity Framework Core
- **AI Service:** Groq AI (openai/gpt-oss-120b) via REST API
- **Database:** SQL Server (ChatMessage, UserActionLogs)
- **Frontend:** React (Web), Flutter (Mobile)
- **Authentication:** JWT Bearer Token

---

## 2. Kiến trúc tổng thể

### 📐 Architecture Diagram

```mermaid
graph TB
    subgraph "Client Layer"
        A1[React Web App]
        A2[Flutter Mobile App]
    end
    
    subgraph "API Gateway"
        B[ChatController<br/>POST /api/Chat/ask<br/>GET /api/Chat/history]
    end
    
    subgraph "Application Layer - MediatR CQRS"
        C1[ChatWithBotQuery<br/>Handler]
        C2[GetChatHistoryQuery<br/>Handler]
    end
    
    subgraph "Domain Services"
        D1[IAiChatService<br/>GroqAiChatService]
    end
    
    subgraph "External Services"
        E[Groq AI API<br/>openai/gpt-oss-120b]
    end
    
    subgraph "Data Access Layer"
        F1[(SQL Server<br/>ChatMessage)]
        F2[(SQL Server<br/>UserProfiles)]
        F3[(SQL Server<br/>NutritionLogs)]
        F4[(SQL Server<br/>WorkoutLogs)]
        F5[(SQL Server<br/>UserActionLogs)]
    end
    
    A1 -->|HTTP POST + JWT| B
    A2 -->|HTTP POST + JWT| B
    B -->|MediatR.Send| C1
    B -->|MediatR.Send| C2
    C1 -->|BuildUserContext| F2
    C1 -->|Query 7 days| F3
    C1 -->|Query 7 days| F4
    C1 -->|Top 50 actions| F5
    C1 -->|SaveMessage| F1
    C1 -->|GetHealthAdvice| D1
    D1 -->|REST API Call| E
    E -->|AI Response| D1
    D1 -->|Response| C1
    C1 -->|SaveAIMessage| F1
    C2 -->|GetHistory| F1
    
    style A1 fill:#4CAF50
    style A2 fill:#4CAF50
    style B fill:#2196F3
    style C1 fill:#FF9800
    style C2 fill:#FF9800
    style D1 fill:#9C27B0
    style E fill:#F44336
    style F1 fill:#607D8B
    style F2 fill:#607D8B
    style F3 fill:#607D8B
    style F4 fill:#607D8B
    style F5 fill:#607D8B
```

### 🔄 Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                   Presentation Layer                         │
│   - ChatController (API Endpoints)                          │
│   - JWT Authentication Middleware                           │
└────────────┬────────────────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────────────────┐
│                  Application Layer                           │
│   - ChatWithBotQueryHandler (Business Logic)                │
│   - GetChatHistoryQueryHandler                              │
│   - DTOs (ChatRequestDto, ChatResponseDto)                  │
└────────────┬────────────────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────────────────┐
│                    Domain Layer                              │
│   - IAiChatService (Interface)                              │
│   - ChatMessage, UserProfile (Entities)                     │
└────────────┬────────────────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────────────────┐
│                 Infrastructure Layer                         │
│   - GeminiAiChatService (Implementation)                    │
│   - ApplicationDbContext (EF Core)                          │
│   - Repository Pattern                                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Chi tiết luồng hoạt động

### 🔄 Complete Request Flow

#### **Step 1: User gửi câu hỏi từ Frontend**

**Web (React + TypeScript):**
```typescript
// File: HealthSync_web/src/services/chatService.ts

export const chatService = {
  sendMessage: async (question: string): Promise<ChatResponse> => {
    const request: ChatRequest = { question };
    const response = await chatApi.post<ChatResponse>('/ask', request);
    return response.data;
  }
};

// Usage in Component
const response = await chatService.sendMessage("Tôi nên ăn gì để tăng cân?");
```

**Mobile (Flutter + Dart):**
```dart
// File: HealthSync_mobile/lib/services/chat_service.dart

class ChatService {
  Future<ChatResponse> sendMessage(String question) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/Chat/ask'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token'
      },
      body: jsonEncode({'question': question}),
    );
    return ChatResponse.fromJson(jsonDecode(response.body));
  }
}
```

**HTTP Request:**
```http
POST /api/Chat/ask HTTP/1.1
Host: localhost:8080
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "question": "Tôi nên ăn gì để tăng cân?"
}
```

---

#### **Step 2: API Gateway - Authentication & Routing**

**File:** `backend/HealthSync.Presentation/Controllers/ChatController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // ✅ Require JWT Authentication
public class ChatController : ControllerBase
{
    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponseDto>> AskHealthBot([FromBody] ChatRequestDto request)
    {
        // 1. Extract UserId from JWT Token
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        // 2. Validate input
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question cannot be empty");
        
        // 3. Send query to MediatR
        var query = new ChatWithBotQuery { UserId = userId, Question = request.Question };
        var response = await _mediator.Send(query);
        
        return Ok(response);
    }
}
```

**Key Points:**
- ✅ `[Authorize]` → Require valid JWT token
- ✅ `ClaimTypes.NameIdentifier` → Extract UserId from token
- ✅ MediatR pattern → Decouple controller from business logic

---

#### **Step 3: MediatR Handler - Build Context & Call AI**

**File:** `backend/HealthSync.Application/Handlers/ChatWithBotQueryHandler.cs`

```csharp
public class ChatWithBotQueryHandler : IRequestHandler<ChatWithBotQuery, ChatResponseDto>
{
    public async Task<ChatResponseDto> Handle(ChatWithBotQuery request, CancellationToken ct)
    {
        // ═══════════════════════════════════════════════════════════
        // STEP 3.1: Build Comprehensive User Context
        // ═══════════════════════════════════════════════════════════
        var userContext = await BuildUserContextAsync(request.UserId, ct);
        var contextJson = JsonSerializer.Serialize(userContext, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true 
        });
        
        // ═══════════════════════════════════════════════════════════
        // STEP 3.2: Save User Message to Database
        // ═══════════════════════════════════════════════════════════
        var userMessage = new ChatMessage
        {
            ChatMessageId = Guid.NewGuid(),
            UserId = request.UserId,
            Role = "user",
            Content = request.Question,
            CreatedAt = DateTime.UtcNow,
            ContextData = contextJson // 💾 Snapshot of user data at request time
        };
        _context.Add(userMessage);
        
        // ═══════════════════════════════════════════════════════════
        // STEP 3.3: Call AI Service (Gemini)
        // ═══════════════════════════════════════════════════════════
        var aiResponse = await _aiChatService.GetHealthAdviceAsync(
            contextJson, 
            request.Question, 
            ct
        );
        
        // ═══════════════════════════════════════════════════════════
        // STEP 3.4: Save AI Response to Database
        // ═══════════════════════════════════════════════════════════
        var assistantMessage = new ChatMessage
        {
            ChatMessageId = Guid.NewGuid(),
            UserId = request.UserId,
            Role = "assistant",
            Content = aiResponse,
            CreatedAt = DateTime.UtcNow
        };
        _context.Add(assistantMessage);
        await _context.SaveChangesAsync(ct);
        
        return new ChatResponseDto
        {
            Response = aiResponse,
            Timestamp = assistantMessage.CreatedAt,
            MessageId = assistantMessage.ChatMessageId
        };
    }
}
```

---

#### **Step 4: Build User Context - Data Aggregation**

**Method:** `BuildUserContextAsync` trong `ChatWithBotQueryHandler`

```csharp
private async Task<UserContextDto> BuildUserContextAsync(int userId, CancellationToken ct)
{
    var context = new UserContextDto();
    
    // ═══════════════════════════════════════════════════════════
    // 📊 1. GET USER PROFILE (BMI, BMR, Age...)
    // ═══════════════════════════════════════════════════════════
    var profile = await _context.UserProfiles
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.UserId == userId, ct);
    
    if (profile != null)
    {
        var age = DateTime.Now.Year - profile.Dob.Year;
        var bmr = CalculateBMR(profile.Gender, profile.WeightKg, profile.HeightCm, age);
        var bmi = CalculateBMI(profile.WeightKg, profile.HeightCm);
        
        context.Profile = new ProfileContextDto
        {
            Gender = profile.Gender,
            Age = age,
            HeightCm = profile.HeightCm,
            CurrentWeightKg = profile.WeightKg,
            Bmr = bmr,  // Basal Metabolic Rate
            Bmi = bmi,  // Body Mass Index
            BmiStatus = GetBMIStatus(bmi), // "Normal", "Overweight", etc.
            ActivityLevel = profile.ActivityLevel
        };
    }
    
    // ═══════════════════════════════════════════════════════════
    // 🎯 2. GET ACTIVE GOAL (Target weight, deadline)
    // ═══════════════════════════════════════════════════════════
    var goal = await _context.Goals
        .Include(g => g.ProgressRecords)
        .Where(g => g.UserId == userId && g.Status == "in_progress")
        .OrderByDescending(g => g.StartDate)
        .FirstOrDefaultAsync(ct);
    
    if (goal != null)
    {
        context.Goal = new GoalContextDto
        {
            Type = goal.Type, // "weight_loss", "weight_gain", "maintain"
            TargetWeightKg = goal.TargetValue,
            Deadline = goal.EndDate
        };
    }
    
    // ═══════════════════════════════════════════════════════════
    // 🔍 3. DATA WAREHOUSE: GET 50 RECENT ACTIONS
    // ═══════════════════════════════════════════════════════════
    var recentActions = await _context.UserActionLogs
        .AsNoTracking()
        .Where(a => a.UserId == userId)
        .OrderByDescending(a => a.Timestamp)
        .Take(50)
        .Select(a => new { a.Timestamp, a.Description })
        .ToListAsync(ct);
    
    if (recentActions.Any())
    {
        context.RecentActivityLogs = string.Join("\n", recentActions.Select(a => 
            $"- [{a.Timestamp:dd/MM HH:mm}] {a.Description}"));
    }
    
    // ═══════════════════════════════════════════════════════════
    // 🍎 4. GET NUTRITION LOGS (Last 7 days)
    // ═══════════════════════════════════════════════════════════
    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;
    var nutritionLogs = await _context.NutritionLogs
        .Include(n => n.FoodEntries)
        .Where(n => n.UserId == userId && n.LogDate >= sevenDaysAgo)
        .ToListAsync(ct);
    
    // ═══════════════════════════════════════════════════════════
    // 🏋️ 5. GET WORKOUT LOGS (Last 7 days)
    // ═══════════════════════════════════════════════════════════
    var workoutLogs = await _context.WorkoutLogs
        .Include(w => w.ExerciseSessions)
            .ThenInclude(es => es.Exercise)
        .Where(w => w.UserId == userId && w.WorkoutDate >= sevenDaysAgo)
        .ToListAsync(ct);
    
    // ═══════════════════════════════════════════════════════════
    // 📅 6. BUILD DAILY LOGS (7 days summary)
    // ═══════════════════════════════════════════════════════════
    for (var date = sevenDaysAgo; date <= DateTime.UtcNow.Date; date = date.AddDays(1))
    {
        var dailyLog = new DailyLogContextDto { Date = date };
        
        // Nutrition for this day
        var nutritionLog = nutritionLogs.FirstOrDefault(n => n.LogDate.Date == date);
        if (nutritionLog != null && nutritionLog.FoodEntries.Any())
        {
            dailyLog.Nutrition = new NutritionContextDto
            {
                Calories = nutritionLog.FoodEntries.Sum(f => f.CaloriesKcal ?? 0),
                ProteinG = nutritionLog.FoodEntries.Sum(f => f.ProteinG ?? 0),
                CarbsG = nutritionLog.FoodEntries.Sum(f => f.CarbsG ?? 0),
                FatG = nutritionLog.FoodEntries.Sum(f => f.FatG ?? 0)
            };
        }
        
        // Workout for this day
        var workoutLog = workoutLogs.FirstOrDefault(w => w.WorkoutDate.Date == date);
        dailyLog.Workout = workoutLog != null
            ? new WorkoutContextDto
              {
                  Status = "Completed",
                  DurationMin = workoutLog.DurationMin,
                  Focus = workoutLog.ExerciseSessions.Select(es => es.Exercise.MuscleGroup).Distinct().ToList()
              }
            : new WorkoutContextDto { Status = "Rest" };
        
        context.RecentLogsLast7Days.Add(dailyLog);
    }
    
    return context;
}
```

**Output Example (UserContextDto JSON):**
```json
{
  "profile": {
    "gender": "Male",
    "age": 25,
    "heightCm": 175,
    "currentWeightKg": 70,
    "bmr": 1680,
    "bmi": 22.86,
    "bmiStatus": "Normal",
    "activityLevel": "moderate"
  },
  "goal": {
    "type": "weight_gain",
    "targetWeightKg": 75,
    "deadline": "2026-03-01"
  },
  "recentActivityLogs": "- [05/01 14:30] Logged Pizza 800 kcal\n- [05/01 09:15] Completed Chest Workout 45 min\n- [04/01 20:00] Viewed Weekly Report",
  "recentLogsLast7Days": [
    {
      "date": "2025-12-30",
      "nutrition": { "calories": 2100, "proteinG": 120, "carbsG": 250, "fatG": 70 },
      "workout": { "status": "Completed", "durationMin": 60, "focus": ["Chest", "Triceps"] }
    }
    // ... 6 more days
  ]
}
```

---

#### **Step 5: AI Service - Call Groq AI**

**File:** `backend/HealthSync.Infrastructure/Services/GroqAiChatService.cs`

```csharp
public class GeminiAiChatService : IAiChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    
    public GeminiAiChatService(IConfiguration configuration)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                     ?? configuration["Gemini:ApiKey"];
        var modelId = configuration["Gemini:ModelId"] ?? "gemini-1.5-flash";
        
        var builder = Kernel.CreateBuilder();
        builder.AddGoogleAIGeminiChatCompletion(modelId: modelId, apiKey: apiKey);
        
        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }
    
    public async Task<string> GetHealthAdviceAsync(
        string userContextData, 
        string userQuestion, 
        CancellationToken ct = default)
    {
        var history = new ChatHistory();
        
        // ═══════════════════════════════════════════════════════════
        // 🧠 PARSE CONTEXT & BUILD ENHANCED SYSTEM PROMPT
        // ═══════════════════════════════════════════════════════════
        var contextObj = JsonSerializer.Deserialize<JsonElement>(userContextData);
        
        // Extract profile data
        string profileData = ExtractProfileData(contextObj);
        string goalData = ExtractGoalData(contextObj);
        string activityLogs = ExtractActivityLogs(contextObj);
        
        // ═══════════════════════════════════════════════════════════
        // 🎯 SYSTEM PROMPT (Ultimate Prompt Engineering)
        // ═══════════════════════════════════════════════════════════
        string systemPrompt = $@"
🏋️‍♂️ Bạn là HealthSync Coach - Trợ lý sức khỏe chuyên nghiệp, thấu hiểu và luôn động viên.

╔══════════════════════════════════════════════════════════════╗
║                    HỒ SƠ CÁ NHÂN                            ║
╚══════════════════════════════════════════════════════════════╝
{profileData}

╔══════════════════════════════════════════════════════════════╗
║                    MỤC TIÊU HIỆN TẠI                         ║
╚══════════════════════════════════════════════════════════════╝
{goalData}

╔══════════════════════════════════════════════════════════════╗
║              NHẬT KÝ HOẠT ĐỘNG GẦN ĐÂY (7 NGÀY)             ║
║         (Data Warehouse - Phân tích kỹ để hiểu thói quen)   ║
╚══════════════════════════════════════════════════════════════╝
{(string.IsNullOrWhiteSpace(activityLogs) ? ""Chưa có dữ liệu thao tác."" : activityLogs)}

╔══════════════════════════════════════════════════════════════╗
║                    HƯỚNG DẪN TRẢ LỜI                         ║
╚══════════════════════════════════════════════════════════════╝
✅ LUÔN LÀM:
1. Trả lời ngắn gọn (100-150 từ), súc tích
2. CÁ NHÂN HÓA: Luôn kết nối với dữ liệu thực tế (Ví dụ: 'Thấy bạn vừa tập...', 'Với BMI hiện tại là...')
3. CHỦ ĐỘNG: Dựa vào logs để khen ngợi (vừa tập) hoặc nhắc nhở nhẹ nhàng (lâu không tập, ăn nhiều calo)
4. HÀNH ĐỘNG CỤ THỂ: Đưa ra số liệu rõ ràng ('Nên ăn thêm 30g protein', 'Giảm 200 kcal/ngày')
5. ĐỘNG VIÊN: Dùng emoji phù hợp, giọng điệu tích cực 💪🔥✨

❌ KHÔNG BAO GIỜ:
1. Trả lời chung chung như Google Search
2. Đưa ra chẩn đoán y khoa (khuyên gặp bác sĩ nếu vấn đề nghiêm trọng)
3. Trả lời câu hỏi không liên quan sức khỏe/thể thao
4. Bỏ qua dữ liệu người dùng đã cung cấp

════════════════════════════════════════════════════════════════
Bây giờ hãy trả lời câu hỏi của người dùng dựa trên TẤT CẢ thông tin trên.
════════════════════════════════════════════════════════════════";
        
        history.AddSystemMessage(systemPrompt);
        history.AddUserMessage(userQuestion);
        
        // ═══════════════════════════════════════════════════════════
        // 🚀 CALL GEMINI API
        // ═══════════════════════════════════════════════════════════
        var executionSettings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                { "maxOutputTokens", 500 },
                { "temperature", 0.7 }  // Balance creativity & accuracy
            }
        };
        
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: executionSettings,
            cancellationToken: ct
        );
        
        return result.Content ?? "Xin lỗi, tôi không thể xử lý yêu cầu này.";
    }
}
```

**Key Techniques:**
- ✅ **Context Injection**: Inject full user profile + activity logs vào system prompt
- ✅ **Prompt Engineering**: Structured prompt với format rõ ràng, hướng dẫn cụ thể
- ✅ **Temperature = 0.7**: Balance giữa creative response và accuracy
- ✅ **MaxTokens = 500**: Giới hạn response length để tránh quá dài

---

#### **Step 6: Save to Database & Return Response**

**Database Operations:**
```csharp
// Save user message
var userMessage = new ChatMessage
{
    ChatMessageId = Guid.NewGuid(),
    UserId = userId,
    Role = "user",
    Content = "Tôi nên ăn gì để tăng cân?",
    CreatedAt = DateTime.UtcNow,
    ContextData = contextJson  // 💾 Snapshot of user data
};
_context.Add(userMessage);

// Save AI response
var assistantMessage = new ChatMessage
{
    ChatMessageId = Guid.NewGuid(),
    UserId = userId,
    Role = "assistant",
    Content = aiResponse,
    CreatedAt = DateTime.UtcNow
};
_context.Add(assistantMessage);

await _context.SaveChangesAsync();
```

**Return to Client:**
```json
{
  "response": "Thấy bạn vừa tập Chest 45 phút rất tốt! 💪 Với BMI hiện tại 22.86 (Normal) và mục tiêu tăng cân lên 75kg, bạn cần surplus 300-500 kcal/ngày. Nên ăn:\n\n1. Protein cao: Thịt gà, cá hồi, trứng (150g protein/ngày)\n2. Carbs phức: Gạo lứt, yến mạch, khoai lang\n3. Chất béo lành mạnh: Bơ, hạt điều, dầu olive\n\nBữa phụ: Chuối + bơ đậu phộng sau tập rất hiệu quả! 🍌🥜",
  "timestamp": "2026-01-05T14:30:00Z",
  "messageId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

## 4. Context Data & AI Prompt Engineering

### 📊 UserContextDto Structure

**File:** `backend/HealthSync.Application/DTOs/UserContextDto.cs`

```csharp
public class UserContextDto
{
    public ProfileContextDto Profile { get; set; } = new();
    public GoalContextDto? Goal { get; set; }
    public List<DailyLogContextDto> RecentLogsLast7Days { get; set; } = new();
    public string? RecentActivityLogs { get; set; }  // ⭐ NEW: Data Warehouse
}

public class ProfileContextDto
{
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal HeightCm { get; set; }
    public decimal CurrentWeightKg { get; set; }
    public decimal Bmr { get; set; }  // Basal Metabolic Rate
    public decimal Bmi { get; set; }  // Body Mass Index
    public string BmiStatus { get; set; } = string.Empty;  // "Underweight", "Normal", "Overweight", "Obese"
    public string ActivityLevel { get; set; } = string.Empty;
}

public class GoalContextDto
{
    public string Type { get; set; } = string.Empty;  // "weight_loss", "weight_gain", "maintain"
    public decimal TargetWeightKg { get; set; }
    public DateTime? Deadline { get; set; }
}

public class DailyLogContextDto
{
    public DateTime Date { get; set; }
    public NutritionContextDto? Nutrition { get; set; }
    public WorkoutContextDto? Workout { get; set; }
}

public class NutritionContextDto
{
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}

public class WorkoutContextDto
{
    public string Status { get; set; } = "Rest";  // "Completed" or "Rest"
    public int? DurationMin { get; set; }
    public List<string> Focus { get; set; } = new();  // ["Chest", "Triceps"]
    public string? Notes { get; set; }
}
```

### 🎯 Prompt Engineering Strategy

**Key Principles:**
1. **Structured Context Injection**: Format dữ liệu người dùng rõ ràng, dễ đọc cho AI
2. **Clear Instructions**: Đưa ra hướng dẫn cụ thể về cách trả lời (✅ LUÔN LÀM, ❌ KHÔNG BAO GIỜ)
3. **Personality & Tone**: Định nghĩa giọng điệu (động viên, thấu hiểu, chuyên nghiệp)
4. **Examples-based**: Đưa ví dụ về cách trả lời tốt vs không tốt
5. **Context Awareness**: AI phải sử dụng dữ liệu thực của user, không trả lời chung chung

**Prompt Template:**
```
🏋️‍♂️ Bạn là HealthSync Coach - Trợ lý sức khỏe chuyên nghiệp...

╔══════════════════════════════════════════╗
║         HỒ SƠ CÁ NHÂN                    ║
╚══════════════════════════════════════════╝
[Inject profile data: BMI, BMR, Age...]

╔══════════════════════════════════════════╗
║        MỤC TIÊU HIỆN TẠI                 ║
╚══════════════════════════════════════════╝
[Inject goal data: target weight, deadline]

╔══════════════════════════════════════════╗
║    NHẬT KÝ HOẠT ĐỘNG GẦN ĐÂY (7 NGÀY)   ║
╚══════════════════════════════════════════╝
[Inject activity logs: recent actions]

╔══════════════════════════════════════════╗
║          HƯỚNG DẪN TRẢ LỜI               ║
╚══════════════════════════════════════════╝
✅ LUÔN LÀM:
1. Trả lời ngắn gọn (100-150 từ)
2. CÁ NHÂN HÓA: Kết nối với dữ liệu thực
3. CHỦ ĐỘNG: Khen ngợi hoặc nhắc nhở
4. HÀNH ĐỘNG CỤ THỂ: Đưa ra số liệu rõ ràng
5. ĐỘNG VIÊN: Emoji + giọng điệu tích cực

❌ KHÔNG BAO GIỜ:
1. Trả lời chung chung
2. Chẩn đoán y khoa
3. Bỏ qua dữ liệu user
```

---

## 5. Components và Implementation

### 🧩 Backend Components

#### **5.1. ChatController (Presentation Layer)**

**File:** `backend/HealthSync.Presentation/Controllers/ChatController.cs`

**Responsibilities:**
- ✅ Receive HTTP requests from clients
- ✅ Validate JWT token & extract UserId
- ✅ Route requests to MediatR handlers
- ✅ Handle exceptions & return appropriate HTTP status codes

**Endpoints:**
```csharp
POST   /api/Chat/ask         → AskHealthBot()
GET    /api/Chat/history     → GetChatHistory()
GET    /api/Chat/health      → HealthCheck()
```

---

#### **5.2. ChatWithBotQueryHandler (Application Layer)**

**File:** `backend/HealthSync.Application/Handlers/ChatWithBotQueryHandler.cs`

**Responsibilities:**
- ✅ Build comprehensive user context from multiple data sources
- ✅ Save user message to database
- ✅ Call AI service with context + question
- ✅ Save AI response to database
- ✅ Return response DTO

**Key Methods:**
```csharp
Handle(ChatWithBotQuery request, CancellationToken ct)
  → BuildUserContextAsync()
  → JsonSerializer.Serialize(userContext)
  → SaveUserMessage()
  → _aiChatService.GetHealthAdviceAsync()
  → SaveAIResponse()
  → Return ChatResponseDto
```

---

#### **5.3. GeminiAiChatService (Infrastructure Layer)**

**File:** `backend/HealthSync.Infrastructure/Services/GroqAiChatService.cs`

**Responsibilities:**
- ✅ Initialize HttpClient with Groq API endpoint
- ✅ Parse user context JSON
- ✅ Build optimized system prompt
- ✅ Call Groq API via REST
- ✅ Return AI response

**Configuration:**
```csharp
// appsettings.json
{
  "Groq": {
    "ApiKey": "YOUR_API_KEY",  // Or set GROQ_API_KEY env variable
    "ModelId": "openai/gpt-oss-120b"
  }
}
```

**Groq API Settings:**
```csharp
var requestBody = new
{
    model = "openai/gpt-oss-120b",
    max_completion_tokens = 8192,
    temperature = 1,
    top_p = 1,
    reasoning_effort = "medium"
};
```

---

### 🎨 Frontend Components

#### **5.4. React Web App**

**File Structure:**
```
HealthSync_web/src/
├── pages/
│   └── ChatScreen.tsx          # Main chat UI
├── services/
│   └── chatService.ts          # API client
└── types/
    └── chat.ts                 # TypeScript interfaces
```

**Key Implementation:**
```typescript
// chatService.ts
export const chatService = {
  sendMessage: async (question: string): Promise<ChatResponse> => {
    const response = await chatApi.post<ChatResponse>('/ask', { question });
    return response.data;
  },
  
  getChatHistory: async (pageSize = 20, pageNumber = 1): Promise<ChatMessage[]> => {
    const response = await chatApi.get<ChatMessage[]>('/history', {
      params: { pageSize, pageNumber }
    });
    return response.data;
  }
};
```

**Chat Screen Features:**
- ✅ Real-time message display
- ✅ Auto-scroll to latest message
- ✅ Loading indicators
- ✅ Error handling
- ✅ Markdown rendering for AI responses

---

#### **5.5. Flutter Mobile App**

**File Structure:**
```
HealthSync_mobile/lib/
├── screens/
│   └── chat_screen.dart        # Chat UI
├── services/
│   └── chat_service.dart       # HTTP client
└── models/
    └── chat_message.dart       # Data models
```

**Key Implementation:**
```dart
// chat_service.dart
class ChatService {
  Future<ChatResponse> sendMessage(String question) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/Chat/ask'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token'
      },
      body: jsonEncode({'question': question}),
    );
    return ChatResponse.fromJson(jsonDecode(response.body));
  }
  
  Future<List<ChatMessage>> getChatHistory({int pageSize = 20, int pageNumber = 1}) async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/Chat/history?pageSize=$pageSize&pageNumber=$pageNumber'),
      headers: {'Authorization': 'Bearer $token'}
    );
    return (jsonDecode(response.body) as List)
        .map((json) => ChatMessage.fromJson(json))
        .toList();
  }
}
```

---

## 6. Database Schema

### 📊 ChatMessage Entity

**File:** `backend/HealthSync.Domain/Entities/ChatMessage.cs`

```csharp
public class ChatMessage
{
    public Guid ChatMessageId { get; set; }              // Primary Key
    public int UserId { get; set; }                      // Foreign Key to ApplicationUser
    public string Role { get; set; }                     // "user" or "assistant"
    public string Content { get; set; }                  // Message text
    public DateTime CreatedAt { get; set; }              // Timestamp
    public string? ContextData { get; set; }             // JSON snapshot of user data
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
}
```

**Table Structure (SQL Server):**
```sql
CREATE TABLE ChatMessages (
    ChatMessageId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId INT NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ContextData NVARCHAR(MAX) NULL,
    
    CONSTRAINT FK_ChatMessages_Users FOREIGN KEY (UserId) REFERENCES ApplicationUsers(UserId)
);

CREATE INDEX IX_ChatMessages_UserId_CreatedAt 
ON ChatMessages(UserId, CreatedAt DESC);
```

---

### 📊 UserActionLog Entity (Data Warehouse)

**Purpose:** Track all user actions for AI analysis

```csharp
public class UserActionLog
{
    public int LogId { get; set; }
    public int UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActionType { get; set; }       // "nutrition_log", "workout_log", "view_report"
    public string Description { get; set; }      // Human-readable description
    public string? Metadata { get; set; }        // Additional JSON data
}
```

**Example Records:**
```json
[
  {
    "logId": 1001,
    "userId": 42,
    "timestamp": "2026-01-05T14:30:00Z",
    "actionType": "nutrition_log",
    "description": "Logged Pizza 800 kcal"
  },
  {
    "logId": 1002,
    "userId": 42,
    "timestamp": "2026-01-05T09:15:00Z",
    "actionType": "workout_log",
    "description": "Completed Chest Workout 45 min"
  },
  {
    "logId": 1003,
    "userId": 42,
    "timestamp": "2026-01-04T20:00:00Z",
    "actionType": "view_report",
    "description": "Viewed Weekly Report"
  }
]
```

---

## 7. API Endpoints

### 📡 API Documentation

#### **7.1. Send Message to AI Chatbot**

```http
POST /api/Chat/ask
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

Request Body:
{
  "question": "Tôi nên ăn gì để tăng cân?"
}

Response (200 OK):
{
  "response": "Thấy bạn vừa tập Chest 45 phút rất tốt! 💪 Với BMI hiện tại 22.86...",
  "timestamp": "2026-01-05T14:30:00Z",
  "messageId": "550e8400-e29b-41d4-a716-446655440000"
}

Error Responses:
- 401 Unauthorized: Invalid or missing JWT token
- 400 Bad Request: Empty question
- 500 Internal Server Error: AI service unavailable
```

---

#### **7.2. Get Chat History**

```http
GET /api/Chat/history?pageSize=20&pageNumber=1
Authorization: Bearer {JWT_TOKEN}

Response (200 OK):
[
  {
    "chatMessageId": "550e8400-e29b-41d4-a716-446655440000",
    "userId": 42,
    "role": "user",
    "content": "Tôi nên ăn gì để tăng cân?",
    "createdAt": "2026-01-05T14:30:00Z",
    "contextData": null
  },
  {
    "chatMessageId": "660e8400-e29b-41d4-a716-446655440001",
    "userId": 42,
    "role": "assistant",
    "content": "Thấy bạn vừa tập Chest 45 phút rất tốt!...",
    "createdAt": "2026-01-05T14:30:05Z",
    "contextData": null
  }
]

Query Parameters:
- pageSize: Number of messages per page (max: 50, default: 20)
- pageNumber: Page number (default: 1)

Error Responses:
- 401 Unauthorized: Invalid or missing JWT token
- 500 Internal Server Error: Database error
```

---

#### **7.3. Health Check**

```http
GET /api/Chat/health

Response (200 OK):
{
  "status": "healthy",
  "service": "HealthSync AI Chatbot",
  "timestamp": "2026-01-05T14:30:00Z"
}
```

---

## 8. Frontend Integration

### 🌐 Web Integration (React)

**Step 1: Install Axios**
```bash
npm install axios
```

**Step 2: Create API Client**
```typescript
// src/services/chatService.ts
import axios from 'axios';

const chatApi = axios.create({
  baseURL: `${process.env.VITE_API_BASE_URL}/api/Chat`,
  headers: { 'Content-Type': 'application/json' }
});

// Add JWT token to all requests
chatApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

**Step 3: Use in Component**
```typescript
// src/pages/ChatScreen.tsx
const [messages, setMessages] = useState<ChatMessage[]>([]);
const [loading, setLoading] = useState(false);

const handleSendMessage = async (question: string) => {
  setLoading(true);
  try {
    const response = await chatService.sendMessage(question);
    setMessages(prev => [...prev, 
      { role: 'user', content: question },
      { role: 'assistant', content: response.response }
    ]);
  } catch (error) {
    console.error('Error sending message:', error);
  } finally {
    setLoading(false);
  }
};
```

---

### 📱 Mobile Integration (Flutter)

**Step 1: Add HTTP Package**
```yaml
# pubspec.yaml
dependencies:
  http: ^1.1.0
```

**Step 2: Create Service**
```dart
// lib/services/chat_service.dart
class ChatService {
  final String baseUrl = 'http://localhost:8080';
  
  Future<ChatResponse> sendMessage(String question) async {
    final token = await _getToken();
    final response = await http.post(
      Uri.parse('$baseUrl/api/Chat/ask'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token'
      },
      body: jsonEncode({'question': question}),
    );
    
    if (response.statusCode == 200) {
      return ChatResponse.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Failed to send message');
    }
  }
}
```

**Step 3: Use in Widget**
```dart
// lib/screens/chat_screen.dart
class _ChatScreenState extends State<ChatScreen> {
  final ChatService _chatService = ChatService();
  List<ChatMessage> _messages = [];
  
  Future<void> _sendMessage(String question) async {
    setState(() {
      _messages.add(ChatMessage(role: 'user', content: question));
    });
    
    try {
      final response = await _chatService.sendMessage(question);
      setState(() {
        _messages.add(ChatMessage(role: 'assistant', content: response.response));
      });
    } catch (e) {
      print('Error: $e');
    }
  }
}
```

---

## 9. Security & Authorization

### 🔒 Authentication Flow

```mermaid
sequenceDiagram
    participant User
    participant Frontend
    participant API
    participant AuthService
    participant ChatHandler
    
    User->>Frontend: Login (email, password)
    Frontend->>API: POST /api/Auth/login
    API->>AuthService: Validate credentials
    AuthService-->>API: Return JWT Token
    API-->>Frontend: { token, userId, role }
    Frontend->>Frontend: Store token in localStorage/SecureStorage
    
    User->>Frontend: Send chat message
    Frontend->>API: POST /api/Chat/ask (+ Bearer Token)
    API->>API: Validate JWT Token
    API->>API: Extract UserId from token
    API->>ChatHandler: Process request with UserId
    ChatHandler-->>API: Return AI response
    API-->>Frontend: { response, timestamp, messageId }
```

### 🛡️ Security Measures

#### **1. JWT Authentication**
```csharp
// Startup.cs - Authentication middleware
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
            )
        };
    });
```

#### **2. Authorization Policy**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // ✅ Require authentication for all endpoints
public class ChatController : ControllerBase
{
    [HttpPost("ask")]
    [Authorize(Roles = "Customer")]  // ✅ Only customers can use chatbot
    public async Task<ActionResult<ChatResponseDto>> AskHealthBot(...)
    {
        // Extract UserId from JWT claims
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        // ...
    }
}
```

#### **3. Input Validation**
```csharp
// Validate user input before processing
if (string.IsNullOrWhiteSpace(request.Question))
{
    return BadRequest(new { message = "Question cannot be empty" });
}

if (request.Question.Length > 1000)
{
    return BadRequest(new { message = "Question too long (max 1000 characters)" });
}
```

#### **4. Rate Limiting (Recommended)**
```csharp
// Install: dotnet add package AspNetCoreRateLimit

services.AddMemoryCache();
services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/Chat/ask",
            Limit = 10,  // Max 10 requests
            Period = "1m" // per 1 minute
        }
    };
});
```

#### **5. Environment Variables (API Keys)**
```bash
# Never commit API keys to Git!
# Use environment variables or Azure Key Vault

# .env file (for development)
GEMINI_API_KEY=your_api_key_here

# Production: Set in Azure App Service Configuration
az webapp config appsettings set --name healthsync-api --settings GEMINI_API_KEY=your_key
```

---

## 10. Error Handling & Best Practices

### ⚠️ Error Handling Strategy

#### **10.1. Backend Error Handling**

```csharp
// ChatController.cs
[HttpPost("ask")]
public async Task<ActionResult<ChatResponseDto>> AskHealthBot([FromBody] ChatRequestDto request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var query = new ChatWithBotQuery { UserId = userId, Question = request.Question };
        var response = await _mediator.Send(query);
        return Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized access attempt");
        return Unauthorized(new { message = "Invalid credentials" });
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "External API error (Gemini)");
        return StatusCode(503, new { message = "AI service temporarily unavailable" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in chatbot");
        return StatusCode(500, new { message = "Internal server error" });
    }
}
```

#### **10.2. AI Service Error Handling**

```csharp
// GeminiAiChatService.cs
public async Task<string> GetHealthAdviceAsync(...)
{
    try
    {
        var result = await _chatCompletionService.GetChatMessageContentAsync(...);
        return result.Content ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Gemini API connection failed");
        throw new Exception("AI service connection error", ex);
    }
    catch (JsonException ex)
    {
        _logger.LogError(ex, "Failed to parse AI response");
        return "Đã có lỗi khi xử lý phản hồi AI. Vui lòng thử lại.";
    }
}
```

#### **10.3. Frontend Error Handling**

**React:**
```typescript
const handleSendMessage = async (question: string) => {
  try {
    const response = await chatService.sendMessage(question);
    setMessages(prev => [...prev, { role: 'assistant', content: response.response }]);
  } catch (error: any) {
    if (error.response?.status === 401) {
      // Redirect to login
      navigate('/login');
    } else if (error.response?.status === 503) {
      setError('AI service is temporarily unavailable. Please try again later.');
    } else {
      setError('An unexpected error occurred. Please try again.');
    }
  }
};
```

**Flutter:**
```dart
Future<void> _sendMessage(String question) async {
  try {
    final response = await _chatService.sendMessage(question);
    setState(() {
      _messages.add(ChatMessage(role: 'assistant', content: response.response));
    });
  } on SocketException {
    _showError('No internet connection');
  } on HttpException {
    _showError('Service unavailable');
  } catch (e) {
    _showError('An error occurred: $e');
  }
}
```

---

### ✅ Best Practices

#### **1. Context Data Management**
- ✅ Always serialize context as JSON with camelCase naming
- ✅ Save context snapshot với mỗi user message để audit trail
- ✅ Limit query scope (7 days for logs, 50 for action logs)
- ✅ Use AsNoTracking() for read-only queries to improve performance

#### **2. AI Prompt Optimization**
- ✅ Inject structured data vào system prompt
- ✅ Provide clear examples of good vs bad responses
- ✅ Set appropriate temperature (0.7 for balanced creativity)
- ✅ Limit maxOutputTokens (500) để tránh response quá dài

#### **3. Database Performance**
- ✅ Use indexes on frequently queried fields (UserId, CreatedAt)
- ✅ Implement pagination for chat history (max 50 per page)
- ✅ Use async/await for all database operations
- ✅ Consider implementing caching for user profiles

#### **4. Security**
- ✅ Always validate JWT tokens
- ✅ Never expose API keys in client-side code
- ✅ Implement rate limiting to prevent abuse
- ✅ Sanitize user input before processing

#### **5. Monitoring & Logging**
- ✅ Log all AI service calls với userId và timestamp
- ✅ Track response times để monitor performance
- ✅ Alert on high error rates hoặc slow responses
- ✅ Use Application Insights hoặc similar tools

---

## 📚 Additional Resources

### 🔗 Related Documentation
- [Google Gemini API Documentation](https://ai.google.dev/docs)
- [Microsoft Semantic Kernel Guide](https://learn.microsoft.com/en-us/semantic-kernel/)
- [MediatR CQRS Pattern](https://github.com/jbogard/MediatR)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)

### 📁 Project Files Reference
```
backend/
├── HealthSync.Presentation/Controllers/ChatController.cs
├── HealthSync.Application/
│   ├── Handlers/ChatWithBotQueryHandler.cs
│   ├── Handlers/GetChatHistoryQueryHandler.cs
│   ├── Queries/ChatWithBotQuery.cs
│   ├── DTOs/ChatRequestDto.cs
│   ├── DTOs/ChatResponseDto.cs
│   └── DTOs/UserContextDto.cs
├── HealthSync.Infrastructure/Services/GeminiAiChatService.cs
├── HealthSync.Domain/
│   ├── Entities/ChatMessage.cs
│   └── Interfaces/IAiChatService.cs

frontend/
├── HealthSync_web/src/
│   ├── pages/ChatScreen.tsx
│   ├── services/chatService.ts
��   └── types/chat.ts
├── HealthSync_mobile/lib/
│   ├── screens/chat_screen.dart
│   ├── services/chat_service.dart
│   └── models/chat_message.dart
```

---

## 🎉 Conclusion

HealthSync AI Chatbot là một hệ thống **cá nhân hóa hoàn toàn**, tận dụng:
- ✅ **Google Gemini 1.5 Flash** cho AI responses chất lượng cao
- ✅ **Data Warehouse Lite** để theo dõi hành vi người dùng
- ✅ **CQRS Pattern** (MediatR) cho clean architecture
- ✅ **Microsoft Semantic Kernel** cho easy AI integration
- ✅ **JWT Authentication** cho bảo mật cao

Hệ thống có thể mở rộng để:
- 🚀 Thêm multi-language support (English, Vietnamese)
- 🚀 Implement voice chat với speech-to-text
- 🚀 Tích hợp wearable device data (Fitbit, Apple Watch)
- 🚀 Add AI-powered meal planning & workout recommendations

---

> **Last Updated:** January 5, 2026  
> **Maintainer:** HealthSync Development Team  
> **Contact:** support@healthsync.com
public async Task<ActionResult<ChatResponseDto>> AskHealthBot([FromBody] ChatRequestDto request)
{
    // 1. Lấy UserId từ JWT Claims
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // 2. Validate request
    if (string.IsNullOrWhiteSpace(request.Question))
        return BadRequest("Question cannot be empty");
    
    // 3. Tạo Query object
    var query = new ChatWithBotQuery 
    { 
        UserId = userId, 
        Question = request.Question 
    };
    
    // 4. Gửi đến Handler qua MediatR
    var response = await _mediator.Send(query);
    
    return Ok(response);
}
```

**Input:**
```json
{
  "question": "Tôi nên ăn gì để tăng cân?"
}
```

**Authorization:**
- JWT Token được validate
- UserId được extract từ Claims
- Chỉ Customer role mới được sử dụng

---

### **3. Handler xử lý Business Logic**

**File:** [backend/HealthSync.Application/Handlers/ChatWithBotQueryHandler.cs](../backend/HealthSync.Application/Handlers/ChatWithBotQueryHandler.cs)

#### **Bước 3.1: Build User Context**

```csharp
private async Task<UserContextDto> BuildUserContextAsync(int userId, CancellationToken ct)
{
    var context = new UserContextDto();
    
    // A. Lấy Profile
    var profile = await _context.UserProfiles
        .FirstOrDefaultAsync(p => p.UserId == userId, ct);
    
    context.Profile = new ProfileContextDto {
        Gender = profile.Gender,
        Age = CalculateAge(profile.Dob),
        HeightCm = profile.HeightCm,
        CurrentWeightKg = profile.WeightKg,
        Bmr = CalculateBMR(...),
        ActivityLevel = profile.ActivityLevel
    };
    
    // B. Lấy Active Goal
    var goal = await _context.Goals
        .Include(g => g.ProgressRecords)
        .Where(g => g.UserId == userId && g.Status == "in_progress")
        .OrderByDescending(g => g.StartDate)
        .FirstOrDefaultAsync(ct);
    
    context.Goal = new GoalContextDto {
        Type = goal.Type,
        TargetWeightKg = goal.TargetValue,
        Deadline = goal.EndDate
    };
    
    // C. Lấy Recent Activity Logs (50 actions gần nhất)
    var recentActions = await _context.UserActionLogs
        .Where(a => a.UserId == userId)
        .OrderByDescending(a => a.Timestamp)
        .Take(50)
        .ToListAsync(ct);
    
    context.RecentActivityLogs = string.Join("\n", recentActions.Select(a => 
        $"- [{a.Timestamp:dd/MM HH:mm}] {a.Description}"
    ));
    
    // D. Lấy Nutrition & Workout Logs (7 ngày gần nhất)
    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;
    
    var nutritionLogs = await _context.NutritionLogs
        .Include(n => n.FoodEntries)
        .Where(n => n.UserId == userId && n.LogDate >= sevenDaysAgo)
        .ToListAsync(ct);
    
    var workoutLogs = await _context.WorkoutLogs
        .Include(w => w.ExerciseSessions)
            .ThenInclude(es => es.Exercise)
        .Where(w => w.UserId == userId && w.WorkoutDate >= sevenDaysAgo)
        .ToListAsync(ct);
    
    // E. Build Daily Logs
    for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
    {
        var dailyLog = new DailyLogContextDto {
            Date = date,
            Nutrition = ..., // Tổng Calories, Protein, Carbs, Fat
            Workout = ...    // Duration, MuscleGroups, Notes
        };
        context.RecentLogsLast7Days.Add(dailyLog);
    }
    
    return context;
}
```

**Output Context JSON:**
```json
{
  "profile": {
    "gender": "Male",
    "age": 25,
    "heightCm": 175,
    "currentWeightKg": 70,
    "bmr": 1650,
    "activityLevel": "Moderate"
  },
  "goal": {
    "type": "Weight Gain",
    "targetWeightKg": 75,
    "deadline": "2026-03-01"
  },
  "recentActivityLogs": "- [04/01 14:30] Đã thêm bữa ăn trưa: Cơm gà\n- [04/01 10:15] Hoàn thành workout Chest & Arms",
  "recentLogsLast7Days": [
    {
      "date": "2026-01-04",
      "nutrition": { "calories": 2800, "proteinG": 150, "carbsG": 300, "fatG": 80 },
      "workout": { "status": "Completed", "durationMin": 60, "focus": ["Chest", "Triceps"] }
    }
  ]
}
```

---

#### **Bước 3.2: Save User Message**

```csharp
var userMessage = new ChatMessage {
    ChatMessageId = Guid.NewGuid(),
    UserId = userId,
    Role = "user",
    Content = request.Question,
    CreatedAt = DateTime.UtcNow,
    ContextData = contextJson // Lưu snapshot dữ liệu
};

_context.Add(userMessage);
```

---

#### **Bước 3.3: Call AI Service**

```csharp
var aiResponse = await _aiChatService.GetHealthAdviceAsync(
    contextJson, 
    request.Question, 
    cancellationToken
);
```

---

### **4. AI Service xử lý với Gemini**

**File:** [backend/HealthSync.Infrastructure/Services/GeminiAiChatService.cs](../backend/HealthSync.Infrastructure/Services/GeminiAiChatService.cs)

```csharp
public class GeminiAiChatService : IAiChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    
    public GeminiAiChatService(IConfiguration config)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                     ?? config["Gemini:ApiKey"];
        
        var builder = Kernel.CreateBuilder();
        builder.AddGoogleAIGeminiChatCompletion(
            modelId: "gemini-1.5-flash",
            apiKey: apiKey
        );
        
        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }
    
    public async Task<string> GetHealthAdviceAsync(
        string userContextData, 
        string userQuestion, 
        CancellationToken ct = default)
    {
        var history = new ChatHistory();
        
        // System Prompt Engineering
        string systemPrompt = $@"
Bạn là HealthSync Bot - Trợ lý sức khỏe cá nhân chuyên nghiệp và thân thiện. 🏋️‍♂️💪

**VAI TRÒ CỦA BẠN:**
- Tư vấn về dinh dưỡng, luyện tập và sức khỏe dựa trên dữ liệu thực tế của người dùng
- Luôn khuyến khích và động viên người dùng đạt mục tiêu
- Đưa ra lời khuyên khoa học, dễ hiểu và có thể thực hiện được

**QUY TẮC TRẢ LỜI:**
1. Trả lời ngắn gọn (3-5 câu), đi thẳng vào vấn đề
2. Sử dụng emoji phù hợp để thân thiện hơn
3. Luôn dựa vào dữ liệu thực tế được cung cấp
4. Nếu thiếu dữ liệu, hãy yêu cầu người dùng nhập thêm
5. Đưa ra gợi ý cụ thể, có số liệu
6. Không đưa ra chẩn đoán y khoa

**DỮ LIỆU NGƯỜI DÙNG (7 NGÀY GẦN NHẤT):**
---
{userContextData}
---

**LỊCH SỬ THAO TÁC GẦN ĐÂY:**
{activityLogs}
";
        
        history.AddSystemMessage(systemPrompt);
        history.AddUserMessage(userQuestion);
        
        // Call Gemini API
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: new PromptExecutionSettings {
                ExtensionData = new Dictionary<string, object> {
                    { "maxOutputTokens", 500 },
                    { "temperature", 0.7 }
                }
            },
            kernel: _kernel,
            cancellationToken: ct
        );
        
        return result.Content ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";
    }
}
```

**AI Response Example:**
```
Chào bạn! 💪 Dựa vào dữ liệu của bạn, mình thấy bạn đang ăn khoảng 2800 calo/ngày 
và tập luyện đều đặn - rất tốt! 🔥

Để tăng cân lên 75kg, bạn nên:
1. Tăng calo lên 3000-3200 kcal/ngày (thêm 200-400 calo)
2. Ưu tiên protein: 160-170g/ngày (thịt, cá, trứng, sữa)
3. Thêm 1-2 bữa phụ với các món như: chuối + bơ đậu phộng, sữa tươi + yến mạch

Tiếp tục duy trì workout 4-5 ngày/tuần nhé! 💪 Cân nặng sẽ tăng từ từ, khoảng 
0.5kg/tuần là lý tưởng. Chúc bạn thành công! 🎯
```

---

#### **Bước 3.4: Save AI Response**

```csharp
var assistantMessage = new ChatMessage {
    ChatMessageId = Guid.NewGuid(),
    UserId = userId,
    Role = "assistant",
    Content = aiResponse,
    CreatedAt = DateTime.UtcNow
};

_context.Add(assistantMessage);
await _context.SaveChangesAsync(cancellationToken);
```

---

#### **Bước 3.5: Return Response**

```csharp
return new ChatResponseDto {
    Response = aiResponse,
    Timestamp = assistantMessage.CreatedAt,
    MessageId = assistantMessage.ChatMessageId
};
```

---

### **5. Frontend nhận và hiển thị Response**

**Web (React):**
```typescript
const handleSendMessage = async (question: string) => {
  // 1. Hiển thị user message
  const userMessage = {
    id: Date.now().toString(),
    role: 'user',
    content: question,
    createdAt: new Date().toISOString()
  };
  setMessages(prev => [...prev, userMessage]);
  
  // 2. Call API
  const response = await chatService.sendMessage(question);
  
  // 3. Hiển thị AI response
  const aiMessage = {
    id: response.messageId,
    role: 'assistant',
    content: response.response,
    createdAt: response.timestamp
  };
  setMessages(prev => [...prev, aiMessage]);
};
```

---

## 📦 Các Components Chính

### **1. Backend Components**

| Component | Path | Responsibility |
|-----------|------|----------------|
| **ChatController** | `HealthSync.Presentation/Controllers/` | API Endpoints, Authorization |
| **ChatWithBotQueryHandler** | `HealthSync.Application/Handlers/` | Business Logic, Context Building |
| **GeminiAiChatService** | `HealthSync.Infrastructure/Services/` | AI Integration (Gemini API) |
| **ChatMessage Entity** | `HealthSync.Domain/Entities/` | Database Model |
| **IAiChatService Interface** | `HealthSync.Domain/Interfaces/` | Service Contract |

### **2. Frontend Components**

| Component | Path | Responsibility |
|-----------|------|----------------|
| **ChatScreen.tsx** | `HealthSync_web/src/pages/` | UI Component |
| **chatService.ts** | `HealthSync_web/src/services/` | API Client |
| **chat.ts** | `HealthSync_web/src/types/` | TypeScript Types |

---

## 🧠 Context Data và AI Prompt

### **User Context Structure**

```typescript
{
  profile: {
    gender: "Male" | "Female",
    age: number,
    heightCm: number,
    currentWeightKg: number,
    bmr: number,
    activityLevel: "Sedentary" | "Light" | "Moderate" | "Active" | "Very Active"
  },
  goal: {
    type: "Weight Loss" | "Weight Gain" | "Muscle Gain" | "Maintain",
    targetWeightKg: number,
    deadline: Date
  },
  recentActivityLogs: string,  // "- [04/01 14:30] Đã thêm bữa ăn..."
  recentLogsLast7Days: [
    {
      date: Date,
      nutrition: {
        calories: number,
        proteinG: number,
        carbsG: number,
        fatG: number
      },
      workout: {
        status: "Completed" | "Rest",
        durationMin: number,
        focus: ["Chest", "Legs"],
        notes: string
      }
    }
  ]
}
```

### **AI Prompt Strategy**

1. **System Prompt:** Định nghĩa role, personality và rules của AI
2. **Context Injection:** Inject dữ liệu người dùng vào prompt
3. **Activity Logs:** Thêm lịch sử thao tác để AI hiểu context
4. **Response Format:** Hướng dẫn AI trả lời ngắn gọn, có emoji
5. **Safety Rules:** Không chẩn đoán y khoa, khuyên gặp bác sĩ nếu cần

---

## 💾 Database Schema

### **ChatMessage Table**

```sql
CREATE TABLE ChatMessages (
    ChatMessageId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId INT NOT NULL,
    Role NVARCHAR(20) NOT NULL,  -- 'user' or 'assistant'
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ContextData NVARCHAR(MAX),  -- JSON snapshot
    
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_ChatMessages_UserId_CreatedAt 
    ON ChatMessages(UserId, CreatedAt DESC);
```

### **Sample Data**

```json
{
  "chatMessageId": "a1b2c3d4-...",
  "userId": 5,
  "role": "user",
  "content": "Tôi nên ăn gì để tăng cân?",
  "createdAt": "2026-01-04T14:30:00Z",
  "contextData": "{\"profile\":{...},\"goal\":{...}}"
}
```

---

## 🔌 API Endpoints

### **1. POST /api/Chat/ask**
**Gửi câu hỏi cho Chatbot**

**Request:**
```http
POST /api/Chat/ask
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "question": "Tôi nên ăn gì để tăng cân?"
}
```

**Response:**
```json
{
  "response": "Chào bạn! 💪 Dựa vào dữ liệu...",
  "timestamp": "2026-01-04T14:30:15Z",
  "messageId": "a1b2c3d4-e5f6-..."
}
```

**Status Codes:**
- `200 OK` - Success
- `400 Bad Request` - Question is empty
- `401 Unauthorized` - Invalid/missing token
- `500 Internal Server Error` - AI service error

---

### **2. GET /api/Chat/history**
**Lấy lịch sử chat**

**Request:**
```http
GET /api/Chat/history?pageSize=20&pageNumber=1
Authorization: Bearer <JWT_TOKEN>
```

**Response:**
```json
[
  {
    "id": "a1b2c3d4-...",
    "role": "user",
    "content": "Tôi nên ăn gì để tăng cân?",
    "createdAt": "2026-01-04T14:30:00Z"
  },
  {
    "id": "e5f6g7h8-...",
    "role": "assistant",
    "content": "Chào bạn! 💪 Dựa vào dữ liệu...",
    "createdAt": "2026-01-04T14:30:15Z"
  }
]
```

---

### **3. GET /api/Chat/health**
**Health check endpoint**

**Request:**
```http
GET /api/Chat/health
```

**Response:**
```json
{
  "status": "healthy",
  "service": "HealthSync Chatbot",
  "timestamp": "2026-01-04T14:30:00Z"
}
```

---

## 🎨 Frontend Implementation

### **ChatScreen Component (React)**

**File:** [HealthSync_web/src/pages/ChatScreen.tsx](../HealthSync_web/src/pages/ChatScreen.tsx)

```tsx
const ChatScreen: React.FC = () => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputMessage, setInputMessage] = useState('');
  const [isSending, setIsSending] = useState(false);

  // Load chat history on mount
  useEffect(() => {
    loadChatHistory();
  }, []);

  const loadChatHistory = async () => {
    const history = await chatService.getChatHistory();
    setMessages(history);
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    const question = inputMessage.trim();
    if (!question || isSending) return;

    // Add user message
    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      role: 'user',
      content: question,
      createdAt: new Date().toISOString()
    };
    setMessages(prev => [...prev, userMessage]);
    setInputMessage('');
    setIsSending(true);

    try {
      // Call API
      const response = await chatService.sendMessage(question);
      
      // Add AI response
      const aiMessage: ChatMessage = {
        id: response.messageId,
        role: 'assistant',
        content: response.response,
        createdAt: response.timestamp
      };
      setMessages(prev => [...prev, aiMessage]);
    } catch (error) {
      alert('Không thể gửi tin nhắn. Vui lòng thử lại.');
    } finally {
      setIsSending(false);
    }
  };

  return (
    <div className="chat-container">
      <div className="messages">
        {messages.map(msg => (
          <MessageBubble key={msg.id} message={msg} />
        ))}
      </div>
      <form onSubmit={handleSendMessage}>
        <input 
          value={inputMessage}
          onChange={e => setInputMessage(e.target.value)}
          placeholder="Đặt câu hỏi về sức khỏe..."
          disabled={isSending}
        />
        <button type="submit" disabled={isSending}>
          Gửi
        </button>
      </form>
    </div>
  );
};
```

### **Chat Service (API Client)**

**File:** [HealthSync_web/src/services/chatService.ts](../HealthSync_web/src/services/chatService.ts)

```typescript
const chatApi = axios.create({
  baseURL: `${API_BASE_URL}/api/Chat`,
  headers: { 'Content-Type': 'application/json' }
});

// Inject JWT token
chatApi.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const chatService = {
  sendMessage: async (question: string): Promise<ChatResponse> => {
    const response = await chatApi.post<ChatResponse>('/ask', { question });
    return response.data;
  },

  getChatHistory: async (pageSize = 20): Promise<ChatMessage[]> => {
    const response = await chatApi.get<ChatMessage[]>('/history', {
      params: { pageSize }
    });
    return response.data;
  }
};
```

---

## 🔒 Security và Authorization

### **1. JWT Authentication**

```csharp
[Authorize] // Yêu cầu JWT Token hợp lệ
public class ChatController : ControllerBase
{
    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponseDto>> AskHealthBot(...)
    {
        // Extract UserId from JWT Claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        // User chỉ có thể chat với data của chính mình
        var query = new ChatWithBotQuery { UserId = userId, ... };
    }
}
```

### **2. Role-Based Access Control**

- ✅ **Customer:** Có thể sử dụng Chatbot
- ❌ **Admin/Manager:** Không cần chatbot (dùng admin dashboard)

### **3. Data Privacy**

- User chỉ truy cập được chat history của chính mình
- Context data được lưu dưới dạng JSON snapshot
- Không chia sẻ data giữa các users

### **4. API Rate Limiting**

```csharp
// Có thể implement rate limiting
services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("chatbot", opt => {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});
```

---

## ⚠️ Error Handling

### **1. Backend Error Handling**

```csharp
[HttpPost("ask")]
public async Task<ActionResult<ChatResponseDto>> AskHealthBot(...)
{
    try
    {
        // Business logic...
        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing chat request");
        return StatusCode(500, new { 
            message = "Đã có lỗi xảy ra. Vui lòng thử lại sau." 
        });
    }
}
```

### **2. AI Service Error Handling**

```csharp
public async Task<string> GetHealthAdviceAsync(...)
{
    try
    {
        var result = await _chatCompletionService.GetChatMessageContentAsync(...);
        return result.Content ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";
    }
    catch (HttpRequestException ex)
    {
        // API error
        throw new InvalidOperationException("Gemini API không khả dụng", ex);
    }
    catch (Exception ex)
    {
        // Unexpected error
        throw new InvalidOperationException("Lỗi xử lý AI", ex);
    }
}
```

### **3. Frontend Error Handling**

```typescript
try {
  const response = await chatService.sendMessage(question);
  // Success...
} catch (error: any) {
  if (error.response?.status === 401) {
    alert('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.');
    // Redirect to login
  } else if (error.response?.status === 500) {
    alert('Lỗi server. Vui lòng thử lại sau.');
  } else {
    alert('Không thể gửi tin nhắn. Kiểm tra kết nối internet.');
  }
}
```

---

## 🧪 Testing

### **1. Unit Tests**

**File:** `HealthSync.Application.Tests/Handlers/ChatWithBotQueryHandlerTests.cs`

```csharp
[Fact]
public async Task Handle_WithValidData_ShouldReturnResponse()
{
    // Arrange
    var handler = new ChatWithBotQueryHandler(_mockContext.Object, _mockAiService.Object);
    var query = new ChatWithBotQuery { UserId = 1, Question = "Test?" };
    
    _mockAiService.Setup(x => x.GetHealthAdviceAsync(It.IsAny<string>(), It.IsAny<string>(), default))
        .ReturnsAsync("Test response");
    
    // Act
    var result = await handler.Handle(query, default);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test response", result.Response);
}
```

### **2. Integration Tests**

**File:** `HealthSync.IntegrationTests/Controllers/ChatControllerIntegrationTests.cs`

```csharp
[Fact]
public async Task AskHealthBot_WithValidToken_ShouldReturn200()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _validToken);
    
    // Act
    var response = await client.PostAsJsonAsync("/api/Chat/ask", 
        new { question = "How to gain weight?" });
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ChatResponseDto>();
    result.Response.Should().NotBeNullOrEmpty();
}
```

---

## 📊 Performance Considerations

### **1. Database Optimization**

```sql
-- Index for fast chat history retrieval
CREATE INDEX IX_ChatMessages_UserId_CreatedAt 
    ON ChatMessages(UserId, CreatedAt DESC);

-- Limit query results
SELECT TOP 20 * FROM ChatMessages 
WHERE UserId = @UserId 
ORDER BY CreatedAt DESC;
```

### **2. Caching Strategy**

```csharp
// Cache user profile for 5 minutes
var cacheKey = $"user_profile_{userId}";
var profile = await _cache.GetOrCreateAsync(cacheKey, async entry => {
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    return await _context.UserProfiles.FindAsync(userId);
});
```

### **3. Async Operations**

```csharp
// Parallel data fetching
var profileTask = _context.UserProfiles.FirstOrDefaultAsync(...);
var goalTask = _context.Goals.Where(...).FirstOrDefaultAsync(...);
var nutritionTask = _context.NutritionLogs.Where(...).ToListAsync(...);
var workoutTask = _context.WorkoutLogs.Where(...).ToListAsync(...);

await Task.WhenAll(profileTask, goalTask, nutritionTask, workoutTask);
```

---

## 🚀 Deployment

### **1. Environment Variables**

```bash
# Production
GROQ_API_KEY=your_actual_groq_api_key_here
Groq__ModelId=openai/gpt-oss-120b

# Development
GROQ_API_KEY=test_key
Groq__ModelId=openai/gpt-oss-120b
```

### **2. Docker Configuration**

```yaml
# docker-compose.yml
services:
  backend:
    environment:
      - GROQ_API_KEY=${GROQ_API_KEY}
      - Groq__ModelId=openai/gpt-oss-120b
```

### **3. Azure App Service**

```bash
az webapp config appsettings set \
  --name healthsync-api \
  --settings GROQ_API_KEY=your_key
```

---

## 📈 Future Enhancements

### **Planned Features:**

1. **Voice Input/Output** 🎤
   - Speech-to-text cho user input
   - Text-to-speech cho AI response

2. **Multilingual Support** 🌍
   - Hỗ trợ tiếng Anh, tiếng Việt
   - Auto-detect language

3. **Image Analysis** 📸
   - Upload ảnh món ăn
   - AI phân tích calories

4. **Conversation Memory** 🧠
   - Lưu context giữa các sessions
   - Chatbot nhớ preferences của user

5. **Personalized Recommendations** 💡
   - Meal plans tự động
   - Workout suggestions

---

## 📞 Support & Maintenance

### **Contact:**
- **Developer:** HealthSync Team
- **Email:** support@healthsync.com
- **Documentation:** [GitHub Wiki](https://github.com/healthsync/docs)

### **Monitoring:**
- Application Insights (Azure)
- Groq API usage dashboard
- Error logging with Serilog

---

## 📚 References

- [Groq AI API Documentation](https://console.groq.com/docs)
- [OpenAI Compatible API](https://platform.openai.com/docs/api-reference)
- [MediatR Pattern](https://github.com/jbogard/MediatR)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Version:** 3.0  
**Last Updated:** 05/01/2026  
**AI Model:** Groq AI (openai/gpt-oss-120b)  
**Status:** ✅ Production Ready
