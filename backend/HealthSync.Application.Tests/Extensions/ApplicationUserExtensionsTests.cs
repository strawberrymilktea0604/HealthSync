using HealthSync.Application.Extensions;
using HealthSync.Domain.Entities;
using Xunit;

namespace HealthSync.Application.Tests.Extensions;

public class ApplicationUserExtensionsTests
{
    [Fact]
    public void GetRoleName_WithSingleRole_ShouldReturnRoleName()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    Role = new Role { RoleName = "Admin" }
                }
            }
        };

        // Act
        var roleName = user.GetRoleName();

        // Assert
        Assert.Equal("Admin", roleName);
    }

    [Fact]
    public void GetRoleName_WithMultipleRoles_ShouldReturnFirstRole()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "Admin" } },
                new UserRole { Role = new Role { RoleName = "Customer" } }
            }
        };

        // Act
        var roleName = user.GetRoleName();

        // Assert
        Assert.Equal("Admin", roleName);
    }

    [Fact]
    public void GetRoleName_WithNoRoles_ShouldReturnCustomer()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>()
        };

        // Act
        var roleName = user.GetRoleName();

        // Assert
        Assert.Equal("Customer", roleName);
    }

    [Fact]
    public void GetRoleName_WithNullRole_ShouldReturnCustomer()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = null }
            }
        };

        // Act
        var roleName = user.GetRoleName();

        // Assert
        Assert.Equal("Customer", roleName);
    }

    [Fact]
    public void HasPermission_WithMatchingPermission_ShouldReturnTrue()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    Role = new Role
                    {
                        RolePermissions = new List<RolePermission>
                        {
                            new RolePermission
                            {
                                Permission = new Permission { PermissionCode = "READ_USERS" }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var hasPermission = user.HasPermission("READ_USERS");

        // Assert
        Assert.True(hasPermission);
    }

    [Fact]
    public void HasPermission_WithoutMatchingPermission_ShouldReturnFalse()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    Role = new Role
                    {
                        RolePermissions = new List<RolePermission>
                        {
                            new RolePermission
                            {
                                Permission = new Permission { PermissionCode = "READ_USERS" }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var hasPermission = user.HasPermission("DELETE_USERS");

        // Assert
        Assert.False(hasPermission);
    }

    [Fact]
    public void HasPermission_WithNoPermissions_ShouldReturnFalse()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    Role = new Role
                    {
                        RolePermissions = new List<RolePermission>()
                    }
                }
            }
        };

        // Act
        var hasPermission = user.HasPermission("READ_USERS");

        // Assert
        Assert.False(hasPermission);
    }

    [Fact]
    public void HasPermission_WithMultipleRolesAndPermissions_ShouldFindPermission()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    Role = new Role
                    {
                        RolePermissions = new List<RolePermission>
                        {
                            new RolePermission
                            {
                                Permission = new Permission { PermissionCode = "READ_USERS" }
                            }
                        }
                    }
                },
                new UserRole
                {
                    Role = new Role
                    {
                        RolePermissions = new List<RolePermission>
                        {
                            new RolePermission
                            {
                                Permission = new Permission { PermissionCode = "WRITE_DATA" }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var hasPermission = user.HasPermission("WRITE_DATA");

        // Assert
        Assert.True(hasPermission);
    }
}
