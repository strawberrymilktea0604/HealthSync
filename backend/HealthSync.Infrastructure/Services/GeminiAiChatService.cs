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

        // Parse context to extract activity logs
        var contextObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(userContextData);
        string activityLogs = "";
        
        if (contextObj.TryGetProperty("recentActivityLogs", out var logsElement) && 
            logsElement.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            activityLogs = logsElement.GetString() ?? "";
        }

        // System Prompt with Context Injection + Activity Logs
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
5. Đưa ra gợi ý cụ thể, có số liệu (ví dụ: ""Hãy tăng protein lên 120g/ngày"")
6. Không đưa ra chẩn đoán y khoa - khuyên họ gặp bác sĩ nếu vấn đề nghiêm trọng

**DỮ LIỆU NGƯỜI DÙNG (7 NGÀY GẦN NHẤT):**
---
{userContextData}
---

**LỊCH SỬ THAO TÁC GẦN ĐÂY (DATA WAREHOUSE - AI CONTEXT):**
{(string.IsNullOrWhiteSpace(activityLogs) ? "Chưa có dữ liệu thao tác." : activityLogs)}

Dựa vào lịch sử thao tác này để hiểu bối cảnh user (ví dụ: vừa tập xong thì khen ngợi, vừa ăn nhiều thì nhắc nhở, lâu không tập thì động viên).

Hãy phân tích dữ liệu trên và trả lời câu hỏi của người dùng một cách chính xác nhất.";

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
