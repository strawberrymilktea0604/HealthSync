using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Queries;

public class GetNutritionLogByDateQuery : IRequest<NutritionLogDto?>
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
}
