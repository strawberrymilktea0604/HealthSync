using MediatR;
using HealthSync.Application.DTOs;

namespace HealthSync.Application.Queries;

public class GetChatHistoryQuery : IRequest<List<ChatHistoryDto>>
{
    public int UserId { get; set; }
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
}
