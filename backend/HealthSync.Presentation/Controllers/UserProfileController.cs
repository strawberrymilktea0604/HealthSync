using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IMediator _mediator;

    public UserProfileController(IUserProfileRepository userProfileRepository, IMediator mediator)
    {
        _userProfileRepository = userProfileRepository;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            return NotFound("Profile not found");
        }

        var dto = new UserProfileDto
        {
            UserId = profile.UserId,
            FullName = profile.FullName,
            Dob = profile.Dob,
            Gender = profile.Gender,
            HeightCm = profile.HeightCm,
            WeightKg = profile.WeightKg,
            ActivityLevel = profile.ActivityLevel,
            AvatarUrl = profile.AvatarUrl
        };

        return Ok(dto);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
    {
        // SECURITY: UserId ALWAYS comes from JWT token, NOT from request body
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            return NotFound("Profile not found");
        }

        // Update properties (UserId is NOT changeable)
        profile.FullName = dto.FullName;
        profile.Dob = dto.Dob;
        profile.Gender = dto.Gender;
        profile.HeightCm = dto.HeightCm;
        profile.WeightKg = dto.WeightKg;
        profile.ActivityLevel = dto.ActivityLevel;
        profile.AvatarUrl = dto.AvatarUrl;

        await _userProfileRepository.UpdateAsync(profile);

        return Ok("Profile updated successfully");
    }

    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest("Only image files are allowed");
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest("File size must be less than 5MB");
        }

        using var stream = file.OpenReadStream();
        var command = new UploadAvatarCommand
        {
            UserId = userId,
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var avatarUrl = await _mediator.Send(command);
        return Ok(new { AvatarUrl = avatarUrl });
    }
}