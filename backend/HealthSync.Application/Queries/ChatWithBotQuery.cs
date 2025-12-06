using MediatR;
using HealthSync.Application.DTOs;

namespace HealthSync.Application.Queries;

public class ChatWithBotQuery : IRequest<ChatResponseDto>
{
    public int UserId { get; set; }
    public string Question { get; set; } = string.Empty;
}
