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
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStorageService _storageService;


    public ExercisesController(IMediator mediator, IStorageService storageService)
    {
        _mediator = mediator;
        _storageService = storageService;
    }

    /// <summary>
    /// Lấy danh sách tất cả bài tập (có filter)
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionCodes.EXERCISE_READ)]
    public async Task<IActionResult> GetExercises(
        [FromQuery] string? muscleGroup,
        [FromQuery] string? difficulty,
        [FromQuery] string? search)
    {
        var query = new GetExercisesQuery
        {
            MuscleGroup = muscleGroup,
            Difficulty = difficulty,
            Search = search
        };

        var exercises = await _mediator.Send(query);
        return Ok(exercises);
    }

    /// <summary>
    /// Lấy thông tin chi tiết 1 bài tập theo ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.EXERCISE_READ)]
    public async Task<IActionResult> GetExerciseById(int id)
    {
        var query = new GetExerciseByIdQuery { ExerciseId = id };
        var exercise = await _mediator.Send(query);

        if (exercise == null)
        {
            return NotFound(new { message = "Không tìm thấy bài tập" });
        }

        return Ok(exercise);
    }

    /// <summary>
    /// Tạo bài tập mới
    /// </summary>
    [HttpPost]
    [RequirePermission(PermissionCodes.EXERCISE_CREATE)]
    public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var exerciseId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetExerciseById), new { id = exerciseId }, new { ExerciseId = exerciseId });
    }

    /// <summary>
    /// Cập nhật thông tin bài tập
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission(PermissionCodes.EXERCISE_UPDATE)]
    public async Task<IActionResult> UpdateExercise(int id, [FromBody] UpdateExerciseCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != command.ExerciseId)
        {
            return BadRequest(new { message = "ID không khớp" });
        }

        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(new { message = "Không tìm thấy bài tập" });
        }

        return Ok(new { message = "Cập nhật bài tập thành công" });
    }

    /// <summary>
    /// Xóa bài tập
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission(PermissionCodes.EXERCISE_DELETE)]
    public async Task<IActionResult> DeleteExercise(int id)
    {
        var command = new DeleteExerciseCommand { ExerciseId = id };

        try
        {
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy bài tập" });
            }

            return Ok(new { message = "Xóa bài tập thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload ảnh cho bài tập
    /// </summary>
    [HttpPut("{id}/image")]
    [RequirePermission(PermissionCodes.EXERCISE_UPDATE)]
    public async Task<IActionResult> UploadExerciseImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "File không hợp lệ" });
        }

        // Kiểm tra xem exercise có tồn tại không
        var query = new GetExerciseByIdQuery { ExerciseId = id };
        var exercise = await _mediator.Send(query);

        if (exercise == null)
        {
            return NotFound(new { message = "Không tìm thấy bài tập" });
        }

        // Upload file lên MinIO (vào folder exercises trong bucket healthsync-files)
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"exercises/{id}_{Guid.NewGuid()}{extension}";

        using var stream = file.OpenReadStream();
        var imageUrl = await _storageService.UploadFileAsync(stream, fileName, file.ContentType);

        // Cập nhật ImageUrl vào database
        var updateCommand = new UpdateExerciseImageCommand
        {
            ExerciseId = id,
            ImageUrl = imageUrl
        };

        await _mediator.Send(updateCommand);

        return Ok(new { imageUrl });
    }}