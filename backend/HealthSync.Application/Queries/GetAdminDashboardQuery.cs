using MediatR;
using HealthSync.Application.DTOs;

namespace HealthSync.Application.Queries;

public class GetAdminDashboardQuery : IRequest<AdminDashboardDto>
{
}
