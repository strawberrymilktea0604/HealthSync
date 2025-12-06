using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries
{
    public class GetCustomerDashboardQuery : IRequest<CustomerDashboardDto>
    {
        public int UserId { get; set; }
    }
}
