using Microsoft.Extensions.Configuration;
using HealthSync.Domain.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthSync.Infrastructure.Services;

public class GroqAiChatService : IAiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelId;

    public GroqAiChatService(IConfiguration configuration)
    {
        // Đọc từ environment variable trước, fallback về appsettings
        _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") 
                  ?? configuration["Groq:ApiKey"] 
                  ?? throw new InvalidOperationException("Groq API Key is not configured. Set GROQ_API_KEY environment variable or Groq:ApiKey in appsettings.json");
        
        _modelId = configuration["Groq:ModelId"] ?? "openai/gpt-oss-120b";
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.groq.com/openai/v1/")
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetHealthAdviceAsync(
        string userContextData, 
        string userQuestion, 
        CancellationToken cancellationToken = default)
    {
        // Parse context to extract detailed user info for optimized prompt
        var contextObj = JsonSerializer.Deserialize<JsonElement>(userContextData);
        
        // Extract data using helper methods (reduces cognitive complexity)
        string activityLogs = ExtractActivityLogs(contextObj);
        string profileData = ExtractProfileData(contextObj, out string bmiStatus);
        string goalData = ExtractGoalData(contextObj);
        
        // System Prompt with Enhanced Context Injection
        string systemPrompt = $@"
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
║              NHẬT KÝ HOẠT ĐỘNG GẦN ĐÂY (7 NGÀY)             ║
║         (Data Warehouse - Phân tích kỹ để hiểu thói quen)   ║
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
thì món này hơi cao so với BMR {profileData}. Chiều nay cố gắng tập Cardio 30 phút 
để tiêu hao nhé! Bạn muốn mình gợi ý bài tập không? 💪'

════════════════════════════════════════════════════════════════
Bây giờ hãy trả lời câu hỏi của người dùng dựa trên TẤT CẢ thông tin trên.
════════════════════════════════════════════════════════════════";

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
            reasoning_effort = "medium",
            stop = (string?)null
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
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

    private class GroqResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private class Message
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
        string age = profileElement.TryGetProperty("age", out var a) ? a.GetInt32().ToString() : "N/A";
        string height = profileElement.TryGetProperty("heightCm", out var h) ? h.GetDecimal().ToString("F1") : "N/A";
        string weight = profileElement.TryGetProperty("currentWeightKg", out var w) ? w.GetDecimal().ToString("F1") : "N/A";
        string bmi = profileElement.TryGetProperty("bmi", out var b) ? b.GetDecimal().ToString("F1") : "N/A";
        bmiStatus = GetJsonStringProperty(profileElement, "bmiStatus");
        string bmr = profileElement.TryGetProperty("bmr", out var bmrVal) ? bmrVal.GetDecimal().ToString("F0") : "N/A";
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
        string targetWeight = goalElement.TryGetProperty("targetWeightKg", out var tw) ? tw.GetDecimal().ToString("F1") : "N/A";
        string deadline = GetJsonStringProperty(goalElement, "deadline");

        return $"- Loại mục tiêu: {goalType}\n- Cân nặng mục tiêu: {targetWeight}kg\n- Thời hạn: {deadline}";
    }

    private static string GetJsonStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() ?? "N/A" : "N/A";
    }
}
