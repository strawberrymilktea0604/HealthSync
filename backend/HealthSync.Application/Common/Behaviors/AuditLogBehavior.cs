using MediatR;
using Microsoft.Extensions.Logging;
using HealthSync.Application.Common.Interfaces;
using HealthSync.Domain.Interfaces;
using HealthSync.Domain.Entities;
using System.Text.Json;

namespace HealthSync.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior để tự động log mọi Command thành công vào bảng UserActionLogs
/// (Lite Data Warehouse cho AI Context)
/// </summary>
public class AuditLogBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AuditLogBehavior<TRequest, TResponse>> _logger;

    public AuditLogBehavior(
        ICurrentUserService currentUserService, 
        IApplicationDbContext context,
        ILogger<AuditLogBehavior<TRequest, TResponse>> logger)
    {
        _currentUserService = currentUserService;
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // 1. Thực thi Logic chính trước
        var response = await next();

        // 2. Sau khi thành công thì ghi Log (chỉ cho Commands, không log Queries)
        try
        {
            var requestTypeName = typeof(TRequest).Name;
            
            // Chỉ log Command (thao tác ghi), bỏ qua Query (thao tác đọc)
            if (!requestTypeName.Contains("Query") && _currentUserService.IsAuthenticated)
            {
                var userId = _currentUserService.UserId;
                if (userId != null && int.TryParse(userId, out int parsedUserId))
                {
                    var actionLog = new UserActionLog
                    {
                        UserId = parsedUserId,
                        ActionType = requestTypeName,
                        Description = GenerateDescription(requestTypeName),
                        MetaDataJson = SerializeMetadata(request),
                        Timestamp = DateTime.UtcNow
                    };

                    _context.Add(actionLog);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation(
                        "[AuditLog] User {UserId} executed {ActionType}", 
                        parsedUserId, 
                        requestTypeName);
                }
            }
        }
        catch (Exception ex)
        {
            // Không để lỗi log làm chết app
            _logger.LogError(ex, "[AuditLog] Failed to log action for request {RequestType}", typeof(TRequest).Name);
        }

        return response;
    }

    /// <summary>
    /// Tạo mô tả dễ đọc cho từng loại action
    /// </summary>
    private string GenerateDescription(string actionType)
    {
        // Mapping các command phổ biến sang mô tả tiếng Việt
        return actionType switch
        {
            var s when s.Contains("CreateWorkoutLog") => "Đã ghi nhận buổi tập luyện mới",
            var s when s.Contains("UpdateWorkoutLog") => "Đã cập nhật buổi tập luyện",
            var s when s.Contains("DeleteWorkoutLog") => "Đã xóa buổi tập luyện",
            
            var s when s.Contains("CreateNutritionLog") || s.Contains("AddNutritionLog") => "Đã ghi nhật ký dinh dưỡng",
            var s when s.Contains("UpdateNutritionLog") => "Đã cập nhật nhật ký dinh dưỡng",
            var s when s.Contains("DeleteNutritionLog") => "Đã xóa nhật ký dinh dưỡng",
            
            var s when s.Contains("CreateGoal") => "Đã đặt mục tiêu mới",
            var s when s.Contains("UpdateGoal") => "Đã cập nhật mục tiêu",
            var s when s.Contains("DeleteGoal") => "Đã xóa mục tiêu",
            var s when s.Contains("CompleteGoal") => "Đã hoàn thành mục tiêu",
            
            var s when s.Contains("UpdateProfile") => "Đã cập nhật thông tin cá nhân",
            
            var s when s.Contains("SendChatMessage") || s.Contains("Chat") => "Đã tương tác với AI Chatbot",
            
            _ => $"Thực hiện thao tác {actionType}"
        };
    }

    /// <summary>
    /// Serialize request data to JSON for metadata (optional, có thể bỏ nếu không cần)
    /// </summary>
    private string? SerializeMetadata(TRequest request)
    {
        try
        {
            // Chỉ lưu metadata cho một số command quan trọng
            var typeName = typeof(TRequest).Name;
            if (typeName.Contains("CreateWorkoutLog") || 
                typeName.Contains("CreateNutritionLog") || 
                typeName.Contains("CreateGoal"))
            {
                return JsonSerializer.Serialize(request, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
        }
        catch
        {
            // Ignore serialization errors
        }
        
        return null;
    }
}
