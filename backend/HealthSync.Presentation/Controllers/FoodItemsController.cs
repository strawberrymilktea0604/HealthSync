using HealthSync.Application.Commands;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FoodItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoodItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách tất cả món ăn (có filter)
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionCodes.FOOD_READ)]
    public async Task<IActionResult> GetFoodItems([FromQuery] string? search)
    {
        var query = new GetFoodItemsQuery { Search = search };
        var foodItems = await _mediator.Send(query);
        return Ok(foodItems);
    }

    /// <summary>
    /// Lấy thông tin chi tiết 1 món ăn theo ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.FOOD_READ)]
    public async Task<IActionResult> GetFoodItemById(int id)
    {
        var query = new GetFoodItemByIdQuery { FoodItemId = id };
        var foodItem = await _mediator.Send(query);

        if (foodItem == null)
        {
            return NotFound(new { message = "Không tìm thấy món ăn" });
        }

        return Ok(foodItem);
    }

    /// <summary>
    /// Tạo món ăn mới
    /// </summary>
    [HttpPost]
    [RequirePermission(PermissionCodes.FOOD_CREATE)]
    public async Task<IActionResult> CreateFoodItem([FromBody] CreateFoodItemCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var foodItemId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFoodItemById), new { id = foodItemId }, new { FoodItemId = foodItemId });
    }

    /// <summary>
    /// Cập nhật thông tin món ăn
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission(PermissionCodes.FOOD_UPDATE)]
    public async Task<IActionResult> UpdateFoodItem(int id, [FromBody] UpdateFoodItemCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != command.FoodItemId)
        {
            return BadRequest(new { message = "ID không khớp" });
        }

        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(new { message = "Không tìm thấy món ăn" });
        }

        return Ok(new { message = "Cập nhật món ăn thành công" });
    }

    /// <summary>
    /// Xóa món ăn
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission(PermissionCodes.FOOD_DELETE)]
    public async Task<IActionResult> DeleteFoodItem(int id)
    {
        var command = new DeleteFoodItemCommand { FoodItemId = id };

        try
        {
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy món ăn" });
            }

            return Ok(new { message = "Xóa món ăn thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
