using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.Extensions.Configuration;
using HealthSync.Domain.Interfaces;

#pragma warning disable SKEXP0070

namespace HealthSync.Infrastructure.Services;

public class GeminiAiChatService : IAiChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public GeminiAiChatService(IConfiguration configuration)
    {
        // Đọc từ environment variable trước, fallback về appsettings
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                     ?? configuration["Gemini:ApiKey"] 
                     ?? throw new InvalidOperationException("Gemini API Key is not configured. Set GEMINI_API_KEY environment variable or Gemini:ApiKey in appsettings.json");
        var modelId = configuration["Gemini:ModelId"] ?? "gemini-1.5-flash";

        var builder = Kernel.CreateBuilder();
        
        builder.AddGoogleAIGeminiChatCompletion(
            modelId: modelId,
            apiKey: apiKey
        );

        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> GetHealthAdviceAsync(
        string userContextData, 
        string userQuestion, 
        CancellationToken cancellationToken = default)
    {
        var history = new ChatHistory();

        // Parse context to extract detailed user info for optimized prompt
        var contextObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(userContextData);
        
        // Extract activity logs
        string activityLogs = "";
        if (contextObj.TryGetProperty("recentActivityLogs", out var logsElement) && 
            logsElement.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            activityLogs = logsElement.GetString() ?? "";
        }
        
        // Extract profile data
        string profileData = "Chưa có thông tin.";
        string bmiStatus = "N/A"; // Declare outside if block to use in prompt
        if (contextObj.TryGetProperty("profile", out var profileElement))
        {
            var gender = profileElement.TryGetProperty("gender", out var g) ? g.GetString() ?? "N/A" : "N/A";
            var age = profileElement.TryGetProperty("age", out var a) ? a.GetInt32().ToString() : "N/A";
            var height = profileElement.TryGetProperty("heightCm", out var h) ? h.GetDecimal().ToString("F1") : "N/A";
            var weight = profileElement.TryGetProperty("currentWeightKg", out var w) ? w.GetDecimal().ToString("F1") : "N/A";
            var bmi = profileElement.TryGetProperty("bmi", out var b) ? b.GetDecimal().ToString("F1") : "N/A";
            bmiStatus = profileElement.TryGetProperty("bmiStatus", out var bs) ? bs.GetString() ?? "N/A" : "N/A";
            var bmr = profileElement.TryGetProperty("bmr", out var bmrVal) ? bmrVal.GetDecimal().ToString("F0") : "N/A";
            var activityLevel = profileElement.TryGetProperty("activityLevel", out var al) ? al.GetString() ?? "N/A" : "N/A";
            
            profileData = $@"- Giới tính: {gender}
- Tuổi: {age}
- Chiều cao: {height}cm | Cân nặng: {weight}kg
- BMI: {bmi} (Trạng thái: {bmiStatus})
- BMR: {bmr} kcal/ngày (Năng lượng tiêu hao cơ bản)
- Mức độ vận động: {activityLevel}";
        }
        
        // Extract goal data
        string goalData = "Chưa thiết lập mục tiêu.";
        if (contextObj.TryGetProperty("goal", out var goalElement) && goalElement.ValueKind != System.Text.Json.JsonValueKind.Null)
        {
            var goalType = goalElement.TryGetProperty("type", out var gt) ? gt.GetString() ?? "N/A" : "N/A";
            var targetWeight = goalElement.TryGetProperty("targetWeightKg", out var tw) ? tw.GetDecimal().ToString("F1") : "N/A";
            var deadline = goalElement.TryGetProperty("deadline", out var dl) ? dl.GetString() ?? "N/A" : "N/A";
            
            goalData = $"- Loại mục tiêu: {goalType}\n- Cân nặng mục tiêu: {targetWeight}kg\n- Thời hạn: {deadline}";
        }
        
        // System Prompt with Enhanced Context Injection (Ultimate Prompt Strategy)
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
        
        history.AddSystemMessage(systemPrompt);
        history.AddUserMessage(userQuestion);

        // Call Gemini API
        var executionSettings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                { "maxOutputTokens", 500 },
                { "temperature", 0.7 }
            }
        };

        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: executionSettings,
            kernel: _kernel,
            cancellationToken: cancellationToken
        );

        return result.Content ?? "Xin lỗi, tôi không thể xử lý câu hỏi của bạn lúc này. Vui lòng thử lại sau. 🙏";
    }
}
