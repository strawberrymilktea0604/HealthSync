using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using HealthSync.Domain.Interfaces;

namespace HealthSync.Infrastructure.Services;

public class GroqAiChatService : IAiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelId;

    public GroqAiChatService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _modelId = configuration["Groq:ModelId"] ?? "openai/gpt-oss-120b";

        // Ensuring BaseAddress is set is primarily the responsibility of DI registration.
        // However, we can perform a check to catch configuration errors early.
        if (_httpClient.BaseAddress == null)
        {
            var baseUrl = configuration["Groq:BaseUrl"];
            // If base URL is provided in config but not set on client, try to set it.
            // This covers scenarios where HttpClient is created manually or via default factory without configuration.
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
            else
            {
                 throw new InvalidOperationException("HttpClient BaseAddress is not configured. Ensure Groq:BaseUrl is set in appsettings.json.");
            }
        }
        
        // Ensure Authorization header is present
        if (_httpClient.DefaultRequestHeaders.Authorization == null)
        {
             var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") 
                   ?? configuration["Groq:ApiKey"];
             
             if (!string.IsNullOrEmpty(apiKey))
             {
                 _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
             }
        }
    }

    public async Task<string> GetHealthAdviceAsync(
        string userContextData, 
        string userQuestion, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse context to extract detailed user info for optimized prompt
            var contextObj = JsonSerializer.Deserialize<JsonElement>(userContextData);
            
            string systemPrompt = BuildSystemPrompt(contextObj);

            var requestBody = new
            {
                model = _modelId,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userQuestion }
                },
                max_completion_tokens = 8192,
                temperature = 1,
                top_p = 1,
                stream = false,
                reasoning_effort = "medium"
            };

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody, jsonOptions),
                Encoding.UTF8,
                "application/json"
            );

            // Use PostAsync with manually serialized content to avoid dependency on System.Net.Http.Json
            var response = await _httpClient.PostAsync("chat/completions", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GroqResponse>(responseContent);

            return result?.Choices?.FirstOrDefault()?.Message?.Content 
                   ?? "Xin lỗi, tôi không thể xử lý câu hỏi của bạn lúc này. Vui lòng thử lại sau. 🙏";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error calling Groq API: {ex.Message}", ex);
        }
    }

    private static string BuildSystemPrompt(JsonElement contextObj)
    {
        // Extract data using helper methods
        string activityLogs = ExtractActivityLogs(contextObj);
        string profileData = ExtractProfileData(contextObj, out string bmiStatus);
        string goalData = ExtractGoalData(contextObj);
        string dailyLogs = ExtractDailyLogs(contextObj);
        string completedGoals = ExtractCompletedGoals(contextObj);
        
        // Safely extract BMR for example template
        string bmrExample = "N/A";
        if (contextObj.TryGetProperty("profile", out var profileElement))
        {
            bmrExample = GetJsonDecimalString(profileElement, "bmr", "F0");
        }

        return $@"
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
{(string.IsNullOrWhiteSpace(completedGoals) ? "Chưa có mục tiêu hoàn thành." : completedGoals)}

╔══════════════════════════════════════════════════════════════╗
║         NHẬT KÝ DINH DƯỠNG & TẬP LUYỆN (7 NGÀY QUA)          ║
╚══════════════════════════════════════════════════════════════╝
{dailyLogs}

╔══════════════════════════════════════════════════════════════╗
║              LỊCH SỬ THAO TÁC HỆ THỐNG                      ║
╚══════════════════════════════════════════════════════════════╝
{(string.IsNullOrWhiteSpace(activityLogs) ? "Chưa có dữ liệu thao tác." : activityLogs)}

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

╔══════════════════════════════════════════════════════════════╗
║                  PHONG CÁCH TRẢ LỜI MẪU                      ║
╚══════════════════════════════════════════════════════════════╝
KHÔNG TỐT: 'Pizza chứa nhiều calo, bạn nên hạn chế.'

RẤT TỐT: 'Mình thấy bạn vừa ăn Pizza 800 kcal 🍕, với BMI hiện tại đang {bmiStatus} 
thì món này hơi cao so với BMR {bmrExample}. Chiều nay cố gắng tập Cardio 30 phút 
để tiêu hao nhé! Bạn muốn mình gợi ý bài tập không? 💪'

════════════════════════════════════════════════════════════════
Bây giờ hãy trả lời câu hỏi của người dùng dựa trên TẤT CẢ thông tin trên.
════════════════════════════════════════════════════════════════";
    }

    private sealed class GroqResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private sealed class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private static string ExtractActivityLogs(JsonElement contextObj)
    {
        if (contextObj.TryGetProperty("recentActivityLogs", out var logsElement) && 
            logsElement.ValueKind == JsonValueKind.String)
        {
            return logsElement.GetString() ?? "";
        }
        return "";
    }

    private static string ExtractProfileData(JsonElement contextObj, out string bmiStatus)
    {
        bmiStatus = "N/A";
        if (!contextObj.TryGetProperty("profile", out var profileElement))
        {
            return "Chưa có thông tin.";
        }

        string gender = GetJsonStringProperty(profileElement, "gender");
        string age = GetJsonNumberString(profileElement, "age");
        string height = GetJsonDecimalString(profileElement, "heightCm", "F1");
        string weight = GetJsonDecimalString(profileElement, "currentWeightKg", "F1");
        string bmi = GetJsonDecimalString(profileElement, "bmi", "F1");
        bmiStatus = GetJsonStringProperty(profileElement, "bmiStatus");
        string bmr = GetJsonDecimalString(profileElement, "bmr", "F0");
        string activityLevel = GetJsonStringProperty(profileElement, "activityLevel");

        return $@"- Giới tính: {gender}
- Tuổi: {age}
- Chiều cao: {height}cm | Cân nặng: {weight}kg
- BMI: {bmi} (Trạng thái: {bmiStatus})
- BMR: {bmr} kcal/ngày (Năng lượng tiêu hao cơ bản)
- Mức độ vận động: {activityLevel}";
    }

    private static string ExtractGoalData(JsonElement contextObj)
    {
        if (!contextObj.TryGetProperty("goal", out var goalElement) || goalElement.ValueKind == JsonValueKind.Null)
        {
            return "Chưa thiết lập mục tiêu.";
        }

        string goalType = GetJsonStringProperty(goalElement, "type");
        string targetWeight = GetJsonDecimalString(goalElement, "targetWeightKg", "F1");
        string deadline = GetJsonStringProperty(goalElement, "deadline");

        return $"- Loại mục tiêu: {goalType}\n- Cân nặng mục tiêu: {targetWeight}kg\n- Thời hạn: {deadline}";
    }

    private static string GetJsonStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() ?? "N/A" : "N/A";
    }

    private static string GetJsonNumberString(JsonElement element, string propertyName)
    {
         if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
         {
             return prop.GetInt32().ToString();
         }
         return "N/A";
    }

    private static string GetJsonDecimalString(JsonElement element, string propertyName, string format)
    {
         if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number)
         {
             return prop.GetDecimal().ToString(format);
         }
         return "N/A";
    }

    private static string ExtractCompletedGoals(JsonElement contextObj)
    {
        if (!contextObj.TryGetProperty("completedGoalsHistory", out var goalsArray) || goalsArray.ValueKind != JsonValueKind.Array)
        {
            return "";
        }

        var sb = new StringBuilder();
        foreach (var p in goalsArray.EnumerateArray())
        {
            sb.AppendLine($"- {p.GetString()}");
        }
        return sb.ToString();
    }

    private static string ExtractDailyLogs(JsonElement contextObj)
    {
        if (!contextObj.TryGetProperty("recentLogsLast7Days", out var logsArray) || logsArray.ValueKind != JsonValueKind.Array)
        {
            return "Chưa có dữ liệu chi tiết.";
        }

        var sb = new StringBuilder();
        foreach (var day in logsArray.EnumerateArray())
        {
            var date = day.TryGetProperty("date", out var d) ? d.GetDateTime().ToString("dd/MM") : "N/A";
            sb.AppendLine($"--- Ngày {date} ---");

            ProcessNutritionLog(day, sb);
            ProcessWorkoutLog(day, sb);
        }

        return sb.Length > 0 ? sb.ToString() : "Không có dữ liệu trong 7 ngày qua.";
    }

    private static void ProcessNutritionLog(JsonElement day, StringBuilder sb)
    {
        if (day.TryGetProperty("nutrition", out var nut) && nut.ValueKind == JsonValueKind.Object)
        {
            var cal = nut.TryGetProperty("calories", out var c) ? c.GetDecimal().ToString("F0") : "0";

            string foodItems = "";
            if (nut.TryGetProperty("foodItems", out var fItems) && fItems.ValueKind == JsonValueKind.Array)
            {
                var items = new List<string>();
                foreach (var item in fItems.EnumerateArray()) items.Add(item.GetString() ?? "");
                foodItems = string.Join(", ", items);
            }

            sb.AppendLine($"   [Ăn uống] {cal} kcal. Món: {foodItems}");
        }
    }

    private static void ProcessWorkoutLog(JsonElement day, StringBuilder sb)
    {
        if (!day.TryGetProperty("workout", out var work) || work.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var status = work.TryGetProperty("status", out var s) ? s.GetString() : "Rest";

        if (status == "Rest" || string.IsNullOrEmpty(status))
        {
            sb.AppendLine("   [Tập luyện] Nghỉ ngơi");
            return;
        }

        var dur = work.TryGetProperty("durationMin", out var dm) ? dm.GetInt32().ToString() : "0";
        string exercises = "";

        if (work.TryGetProperty("exercises", out var exs) && exs.ValueKind == JsonValueKind.Array)
        {
            var items = new List<string>();
            foreach (var item in exs.EnumerateArray())
            {
                items.Add(item.GetString() ?? "");
            }
            exercises = string.Join(", ", items);
        }

        sb.AppendLine($"   [Tập luyện] {status} ({dur} phút). Bài tập: {exercises}");
    }
}
