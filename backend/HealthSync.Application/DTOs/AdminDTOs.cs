namespace HealthSync.Application.DTOs;

public class AdminUserDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
}

public class AdminUserListDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UpdateUserRoleRequest
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserAvatarRequest
{
    public int UserId { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
}

public class AdminUsersResponse
{
    public List<AdminUserListDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
