using MediatR;
using Microsoft.EntityFrameworkCore;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;

namespace HealthSync.Application.Handlers;

public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, List<ChatHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetChatHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatHistoryDto>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.UserId == request.UserId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new ChatHistoryDto
            {
                MessageId = m.ChatMessageId,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return messages.OrderBy(m => m.CreatedAt).ToList();
    }
}
