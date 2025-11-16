using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class GetUserByIdQuery : IRequest<AdminUserDto>
{
    public int UserId { get; set; }
}
