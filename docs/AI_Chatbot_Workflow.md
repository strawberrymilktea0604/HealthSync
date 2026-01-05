# 🤖 Tài liệu Luồng Hoạt động Chatbot AI - HealthSync

## 📋 Mục lục
1. [Tổng quan](#tổng-quan)
2. [Kiến trúc Hệ thống](#kiến-trúc-hệ-thống)
3. [Luồng Hoạt động Chi tiết](#luồng-hoạt-động-chi-tiết)
4. [Các Components Chính](#các-components-chính)
5. [Context Data và AI Prompt](#context-data-và-ai-prompt)
6. [Database Schema](#database-schema)
7. [API Endpoints](#api-endpoints)
8. [Frontend Implementation](#frontend-implementation)
9. [Security và Authorization](#security-và-authorization)
10. [Error Handling](#error-handling)

---

## 🎯 Tổng quan

HealthSync Chatbot là một trợ lý AI thông minh được tích hợp vào hệ thống HealthSync, sử dụng **Google Gemini AI** để cung cấp tư vấn sức khỏe cá nhân hóa dựa trên dữ liệu thực tế của người dùng.

### ✨ Tính năng chính:
- 💬 Tư vấn dinh dưỡng và luyện tập cá nhân hóa
- 📊 Phân tích dữ liệu người dùng (7 ngày gần nhất)
- 🔍 Theo dõi lịch sử thao tác người dùng (Data Warehouse)
- 💾 Lưu trữ lịch sử chat
- 🔒 Bảo mật với JWT Authentication

---

## 🏗️ Kiến trúc Hệ thống

```
┌─────────────────┐
│  Frontend       │
│  React (Web)    │
│  Flutter (App)  │
└────────┬────────┘
         │ HTTP Request
         │ (JWT Token)
         ▼
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway                             │
│                     (ChatController)                         │
└────────┬───────────────────────────────────────────┬────────┘
         │ MediatR                                    │
         ▼                                            ▼
┌────────────────────────┐                  ┌────────────────┐
│  ChatWithBotQuery      │                  │ GetChatHistory │
│  Handler               │                  │ Query          │
└────────┬───────────────┘                  └────────────────┘
         │
         ├─► 1. Build User Context
         │     - Profile (BMR, Age, Weight...)
         │     - Active Goals
         │     - Nutrition Logs (7 days)
         │     - Workout Logs (7 days)
         │     - Recent Activity Logs (50 actions)
         │
         ├─► 2. Save User Message to DB
         │
         ├─► 3. Call AI Service
         │     ┌──────────────────────────────┐
         │     │   GeminiAiChatService        │
         │     │   (Google Gemini 1.5 Flash)  │
         │     └──────────────────────────────┘
         │
         └─► 4. Save AI Response to DB
```

---

## 🔄 Luồng Hoạt động Chi tiết

### **1. User gửi câu hỏi từ Frontend**

**Web (React):**
```typescript
// chatService.ts
chatService.sendMessage(question: string)
  → POST /api/Chat/ask
  → Headers: { Authorization: Bearer <JWT_TOKEN> }
  → Body: { question: "Tôi nên ăn gì để tăng cân?" }
```

**Mobile (Flutter):**
```dart
// chat_service.dart
await chatService.sendMessage(question)
  → HTTP POST /api/Chat/ask
```

---

### **2. API Gateway xử lý Request**

**File:** [backend/HealthSync.Presentation/Controllers/ChatController.cs](../backend/HealthSync.Presentation/Controllers/ChatController.cs)

```csharp
[HttpPost("ask")]
[Authorize] // Kiểm tra JWT Token
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
GEMINI_API_KEY=your_actual_api_key_here
Gemini__ModelId=gemini-1.5-flash

# Development
GEMINI_API_KEY=test_key
Gemini__ModelId=gemini-1.5-flash
```

### **2. Docker Configuration**

```yaml
# docker-compose.yml
services:
  backend:
    environment:
      - GEMINI_API_KEY=${GEMINI_API_KEY}
      - Gemini__ModelId=gemini-1.5-flash
```

### **3. Azure App Service**

```bash
az webapp config appsettings set \
  --name healthsync-api \
  --settings GEMINI_API_KEY=your_key
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
- Gemini API usage dashboard
- Error logging with Serilog

---

## 📚 References

- [Google Gemini API Documentation](https://ai.google.dev/docs)
- [Microsoft Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/)
- [MediatR Pattern](https://github.com/jbogard/MediatR)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Version:** 1.0  
**Last Updated:** 05/01/2026  
**Status:** ✅ Production Ready
