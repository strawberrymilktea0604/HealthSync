using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
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

    public UserProfileController(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
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
    public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
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

        // Update properties
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
}