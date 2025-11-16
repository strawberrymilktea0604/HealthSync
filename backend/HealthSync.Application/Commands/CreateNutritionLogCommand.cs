using HealthSync.Application.DTOs;
using MediatR;

namespace HealthSync.Application.Commands;

public class CreateNutritionLogCommand : IRequest<int>
{
    public int UserId { get; set; }
    public CreateNutritionLogDto NutritionLog { get; set; } = null!;
}