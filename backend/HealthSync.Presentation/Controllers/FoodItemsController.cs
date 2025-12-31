using HealthSync.Application.Commands;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Domain.Interfaces;
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
    private readonly IStorageService _storageService;
    private readonly IConfiguration _configuration;

    public FoodItemsController(IMediator mediator, IStorageService storageService, IConfiguration configuration)
    {
        _mediator = mediator;
        _storageService = storageService;
        _configuration = configuration;
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

    /// <summary>
    /// Upload ảnh cho món ăn
    /// </summary>
    [HttpPut("{id}/image")]
    [RequirePermission(PermissionCodes.FOOD_UPDATE)]
    public async Task<IActionResult> UploadFoodItemImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "File không hợp lệ" });
        }

        // Kiểm tra xem food item có tồn tại không
        var query = new GetFoodItemByIdQuery { FoodItemId = id };
        var foodItem = await _mediator.Send(query);

        if (foodItem == null)
        {
            return NotFound(new { message = "Không tìm thấy món ăn" });
        }

        // Upload file lên MinIO (vào folder foods trong bucket healthsync-files)
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"foods/{id}_{Guid.NewGuid()}{extension}";

        using var stream = file.OpenReadStream();
        var imageUrl = await _storageService.UploadFileAsync(stream, fileName, file.ContentType);

        // Cập nhật ImageUrl vào database
        var updateCommand = new UpdateFoodItemImageCommand
        {
            FoodItemId = id,
            ImageUrl = imageUrl
        };

        await _mediator.Send(updateCommand);

        return Ok(new { imageUrl });
    }
}
