using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries
{
    public class GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
    {
    }
}