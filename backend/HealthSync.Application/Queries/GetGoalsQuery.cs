using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class GetGoalsQuery : IRequest<GetGoalsResponse>
{
    public int UserId { get; set; } // From JWT
}