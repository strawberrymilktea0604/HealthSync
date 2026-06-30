# 🤖 Tài liệu Luồng Hoạt động AI Chatbot - HealthSync

> **Last Updated:** January 6, 2026
> **Version:** 4.0
> **AI Model:** Groq AI (openai/gpt-oss-120b)
> **Status:** Production Ready

---

## 📋 Mục lục

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [Kiến trúc tổng thể](#2-kiến-trúc-tổng-thể)
3. [Chi tiết luồng hoạt động](#3-chi-tiết-luồng-hoạt-động)
4. [Context Data & AI Prompt Engineering](#4-context-data--ai-prompt-engineering)
5. [Components và Implementation](#5-components-và-implementation)
6. [Database Schema](#6-database-schema)
7. [API Endpoints](#7-api-endpoints)

---

## 1. Tổng quan hệ thống

### 🎯 Giới thiệu

**HealthSync AI Chatbot** là trợ lý sức khỏe thông minh, sử dụng **Groq AI** (model `openai/gpt-oss-120b`) với tốc độ phản hồi siêu tốc để cung cấp tư vấn dinh dưỡng và luyện tập **cá nhân hóa 100%**. Hệ thống không chỉ trả lời câu hỏi mà còn đóng vai trò là một "Health Coach" thực thụ, hiểu rõ thể trạng và thói quen của người dùng.

### ✨ Tính năng nổi bật (Updated v4.0)

| Tính năng | Mô tả chi tiết | Công nghệ |
|-----------|----------------|-----------|
| **Siêu cá nhân hóa** | Phân tích BMI, BMR, TDEE và lịch sử hoạt động để đưa ra lời khuyên "đo ni đóng giày" | Context Injection |
| **Data Warehouse Lite** | Ghi nhớ 20 thao tác gần nhất của user (xem bài tập nào, log món ăn gì) để hiểu hành vi | UserActionLogs |
| **Motivational Engine** | Theo dõi "Thành tích đã đạt được" (Completed Goals) để động viên user dựa trên chiến thắng cũ | History Tracking |
| **System Awareness** | AI biết rõ danh sách món ăn và bài tập có sẵn trong hệ thống để gợi ý chính xác | System Resource Mapping |
| **Context Memory** | Nhớ chi tiết 7 ngày ăn uống và tập luyện gần nhất để điều chỉnh thực đơn/bài tập | Rolling Window Log |

### 🛠️ Tech Stack

- **Backend:** ASP.NET Core 8.0, MediatR CQRS
- **AI Service:** Groq Cloud API (openai/gpt-oss-120b)
- **Database:** SQL Server (EF Core)
- **Http Client:** Default HttpClient với Retry Logic
- **Data Format:** JSON Context Snapshot

---

## 2. Kiến trúc tổng thể

### 📐 Architecture Diagram

```mermaid
graph TB
    subgraph "Client Layer"
        A1[React Web App]
        A2[Flutter Mobile App]
    end
    
    subgraph "Presentation Layer"
        B[ChatController]
    end
    
    subgraph "Application Layer (MediatR)"
        C1[ChatWithBotQueryHandler]
    end
    
    subgraph "Domain Services"
        D1[IAiChatService]
        D2[GroqAiChatService]
        D1 <.. D2
    end
    
    subgraph "External Services"
        E[Groq Cloud API]
    end
    
    subgraph "Data Access Layer"
        F1[(SQL Server)]
        T1[UserProfiles]
        T2[Goals]
        T3[NutritionLogs]
        T4[WorkoutLogs]
        T5[UserActionLogs]
        T6[FoodItems/Exercises]
    end
    
    A1 & A2 -->|JWT Auth| B
    B -->|Send Query| C1
    
    C1 -->|1. Build Context| F1
    F1 -.->|Read| T1 & T2 & T3 & T4 & T5 & T6
    
    C1 -->|2. Get Advice| D1
    D1 -->|3. POST Request| E
    E -->|4. AI Response| D1
    
    C1 -->|5. Save Chat| F1
```

---

## 3. Chi tiết luồng hoạt động

### 🔄 Quy trình xử lý Request (MediatR Handler)

File: `backend/HealthSync.Application/Handlers/ChatWithBotQueryHandler.cs`

1.  **Xác thực User:** Kiểm tra User có tồn tại và đang active.
2.  **Tổng hợp Context (Context Aggregation):**
    *   Hệ thống thu thập dữ liệu từ 6 nguồn khác nhau để tạo nên bức tranh toàn cảnh về user.
3.  **Lưu tin nhắn User:** Lưu câu hỏi vào bảng `ChatMessage`.
4.  **Gọi AI Service:** Gửi Context JSON + Câu hỏi sang Groq API.
5.  **Nhận phản hồi & Lưu trữ:** Lưu câu trả lời của AI vào `ChatMessage` và trả về cho Client.

---

## 4. Context Data & AI Prompt Engineering

Đây là phần quan trọng nhất tạo nên sự thông minh của Chatbot. Dữ liệu được gom lại thành object `UserContextDto` và serialize thành JSON.

### 📊 Cấu trúc Context (JSON)

```json
{
  "profile": {
    "gender": "Male",
    "age": 25,
    "heightCm": 175,
    "currentWeightKg": 70,
    "bmr": 1680,
    "bmi": 22.86,
    "bmiStatus": "Bình thường",
    "activityLevel": "Moderate"
  },
  "goal": {
    "type": "weight_gain",
    "targetWeightKg": 75,
    "deadline": "2026-03-01",
    "status": "in_progress",
    "currentProgress": 71.5
  },
  "completedGoalsHistory": [
    "weight_loss: 5kg (Done at 20/12/2025)"
  ],
  "recentActivityLogs": "- [05/01 14:30] Logged Pizza 800 kcal\n- [05/01 09:15] Completed Chest Workout",
  "availableFoodsSummary": [
    "Cơm trắng (130kcal/100g)", "Ức gà (165kcal/100g)"
  ],
  "availableExercisesSummary": [
    "Push Up (Chest) - Beginner", "Squat (Legs) - Intermediate"
  ],
  "recentLogsLast7Days": [
    {
      "date": "2026-01-05",
      "nutrition": { "calories": 2100, "foodItems": ["Cơm", "Gà"] },
      "workout": { "status": "Completed", "durationMin": 60, "focus": ["Chest"] }
    }
  ]
}
```

### 🧠 System Prompt (Prompt Engineering)

Prompt được thiết kế theo cấu trúc "Role-Context-Instruction" để đảm bảo AI không bị "ảo giác" (hallucination).

**Template trong Code (`GroqAiChatService.cs`):**

```text
🏋️‍♂️ Bạn là HealthSync Coach - Trợ lý sức khỏe cá nhân chuyên nghiệp, thấu hiểu và luôn động viên.

╔══════════════════════════════════════════════════════════════╗
║                    HỒ SƠ CÁ NHÂN                            ║
╚══════════════════════════════════════════════════════════════╝
{profileData}

╔══════════════════════════════════════════════════════════════╗
║                    MỤC TIÊU HIỆN TẠI                         ║
╚══════════════════════════════════════════════════════════════╝
{goalData}

╔══════════════════════════════════════════════════════════════╗
║                 THÀNH TÍCH ĐÃ ĐẠT ĐƯỢC                       ║
╚══════════════════════════════════════════════════════════════╝
{completedGoals}

╔══════════════════════════════════════════════════════════════╗
║         NHẬT KÝ DINH DƯỠNG & TẬP LUYỆN (7 NGÀY QUA)          ║
╚══════════════════════════════════════════════════════════════╝
{dailyLogs}

╔══════════════════════════════════════════════════════════════╗
║              LỊCH SỬ THAO TÁC HỆ THỐNG                      ║
╚══════════════════════════════════════════════════════════════╝
{activityLogs}

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
2. Đưa ra chẩn đoán y khoa
3. Bỏ qua dữ liệu người dùng đã cung cấp
...
```

---

## 5. Components và Implementation

### 5.1. GroqAiChatService (`Infrastructure Layer`)

Service chịu trách nhiệm giao tiếp với Groq API. Sử dụng `HttpClient` để gửi request POST tới `chat/completions`.

*   **Config:**
    *   `Groq:ApiKey`: Key API (lấy từ env hoặc appsettings).
    *   `Groq:ModelId`: Mặc định `openai/gpt-oss-120b`.
    *   `Groq:BaseUrl`: Endpoint API của Groq.

*   **Logic xử lý:**
    *   Parse Context JSON.
    *   Tạo System Prompt từ dữ liệu context (Profile, Logs, Goals...).
    *   Call API với `temperature = 1` (Creative nhưng vẫn bám sát context), `max_tokens = 8192`.
    *   Handle Error và trả về message thân thiện nếu lỗi.

### 5.2. ChatWithBotQueryHandler (`Application Layer`)

Handler chịu trách nhiệm logic nghiệp vụ (Business Logic).

*   **Logic xây dựng Context (`BuildUserContextAsync`):**
    1.  **Profile:** Tính tuổi, BMR (Mifflin-St Jeor), BMI, BMI Status (Tiếng Việt).
    2.  **Goal:** Lấy goal đang active (`in_progress`), kèm progress mới nhất.
    3.  **Completed Goals:** Lấy 5 goal đã hoàn thành gần nhất để AI biết lịch sử thành công.
    4.  **System Resources:** Lấy top 40 món ăn và bài tập phổ biến trong DB để AI gợi ý đúng những gì app có.
    5.  **User Actions (Data Warehouse):** Lấy 20 actions gần nhất từ `UserActionLogs`.
    6.  **Detailed Logs (7 days):** Lấy chi tiết nutrition và workout 7 ngày qua, gom nhóm theo ngày.

---

## 6. Database Schema

Các bảng liên quan trực tiếp đến hoạt động của Chatbot:

### `ChatMessage`
Lưu trữ lịch sử hội thoại.
- `ChatMessageId` (PK, Guid)
- `UserId` (FK)
- `Role` ("user" | "assistant")
- `Content` (Nội dung tin nhắn)
- `ContextData` (JSON snapshot tại thời điểm chat - quan trọng để debug)
- `CreatedAt` (DateTime)

### `UserActionLogs` (Data Warehouse Lite)
Lưu vết hành vi user để AI phân tích.
- `LogId` (PK)
- `UserId` (FK)
- `ActionType` (VD: "VIEW_EXERCISE", "LOG_FOOD")
- `Description` (Chi tiết hành động)
- `Timestamp`

---

## 7. API Endpoints

### 1. Chat với Bot
**POST** `/api/Chat/ask`
- **Auth:** Bearer Token
- **Body:** `{ "question": "Hôm nay tôi nên tập gì?" }`
- **Response:**
  ```json
  {
    "response": "Chào bạn! Dựa trên lịch sử hôm qua bạn đã tập Ngực (Chest), hôm nay với mục tiêu tăng cân, mình gợi ý bạn tập Chân (Legs) nhé!...",
    "timestamp": "2026-01-06T13:30:00Z",
    "messageId": "..."
  }
  ```

### 2. Lấy lịch sử Chat
**GET** `/api/Chat/history`
- **Auth:** Bearer Token
- **Response:** List các tin nhắn cũ.

---

> **Note:** Tài liệu này phản ánh chính xác implementation hiện tại trong source code. Khi cập nhật logic code, vui lòng cập nhật tài liệu này tương ứng.
