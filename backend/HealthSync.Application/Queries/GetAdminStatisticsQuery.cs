using MediatR;
using HealthSync.Application.DTOs;

namespace HealthSync.Application.Queries
{
    public class GetAdminStatisticsQuery : IRequest<AdminStatisticsDto>
    {
        public int? Days { get; set; } // Filter statistics by days (e.g., 30, 90, 365)
    }
}
