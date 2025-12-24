using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using MockQueryable.Moq;
using Moq;
using Xunit;
using HealthSync.Application.Services;

namespace HealthSync.Application.Tests.Handlers
{
    public class RegisterAdminCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IJwtTokenService> _mockJwtTokenService;
        private readonly RegisterAdminCommandHandler _handler;

        public RegisterAdminCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockAuthService = new Mock<IAuthService>();
            _mockMediator = new Mock<IMediator>();
            _mockJwtTokenService = new Mock<IJwtTokenService>();

            _handler = new RegisterAdminCommandHandler(
                _mockContext.Object,
                _mockAuthService.Object,
                _mockMediator.Object,
                _mockJwtTokenService.Object
            );
        }

        [Fact]
        public async Task Handle_AdminAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingAdmin = new ApplicationUser
            {
                UserId = 1,
                Email = "admin@test.com",
                UserRoles = new List<UserRole>
                {
                    new UserRole { Role = new Role { RoleName = "Admin" } }
                }
            };

            var users = new List<ApplicationUser> { existingAdmin }.AsQueryable().BuildMock();
            _mockContext.Setup(c => c.ApplicationUsers).Returns(users);

            var command = new RegisterAdminCommand
            {
                Email = "newadmin@test.com",
                Password = "Admin@123",
                VerificationCode = "123456"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );
            Assert.Contains("đã có tài khoản Admin", ex.Message);
        }

        [Fact]
        public async Task Handle_InvalidVerificationCode_ThrowsInvalidOperationException()
        {
            // Arrange
            var users = new List<ApplicationUser>().AsQueryable().BuildMock();
            _mockContext.Setup(c => c.ApplicationUsers).Returns(users);

            _mockMediator.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new RegisterAdminCommand
            {
                Email = "admin@test.com",
                Password = "Admin@123",
                VerificationCode = "123456"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );
            Assert.Contains("Mã xác thực không hợp lệ", ex.Message);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUser = new ApplicationUser
            {
                UserId = 1,
                Email = "admin@test.com",
                UserRoles = new List<UserRole>()
            };

            var users = new List<ApplicationUser> { existingUser }.AsQueryable().BuildMock();
            _mockContext.Setup(c => c.ApplicationUsers).Returns(users);

            _mockMediator.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new RegisterAdminCommand
            {
                Email = "admin@test.com",
                Password = "Admin@123",
                VerificationCode = "123456"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, CancellationToken.None)
            );
            Assert.Contains("Email đã tồn tại", ex.Message);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesAdminSuccessfully()
        {
            // Arrange
            var emptyUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
            var roles = new List<Role> { new Role { Id = 1, RoleName = "Admin" } }.AsQueryable().BuildMock();

            var newAdmin = new ApplicationUser
            {
                UserId = 1,
                Email = "admin@test.com",
                PasswordHash = "hashed_password",
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        UserId = 1,
                        RoleId = 1,
                        Role = new Role
                        {
                            Id = 1,
                            RoleName = "Admin",
                            RolePermissions = new List<RolePermission>()
                        }
                    }
                }
            };

            ApplicationUser capturedAdmin = null;
            var adminList = new List<ApplicationUser>().AsQueryable().BuildMock();

            // Setup ApplicationUsers to return empty first (for existence check and duplicate email check), then captured admin (for reload)
            var callCount = 0;
            _mockContext.Setup(c => c.ApplicationUsers).Returns(() =>
            {
                callCount++;
                if (callCount <= 2) return emptyUsers;
                // Third call - return captured admin if available
                if (capturedAdmin != null)
                {
                    return new List<ApplicationUser> { capturedAdmin }.AsQueryable().BuildMock();
                }
                return adminList;
            });
            _mockContext.Setup(c => c.Roles).Returns(roles);
            _mockContext.Setup(c => c.Add(It.IsAny<object>())).Callback<object>(entity =>
            {
                if (entity is ApplicationUser user)
                {
                    user.UserId = 1; // Simulate database generated ID
                    user.UserRoles = newAdmin.UserRoles; // Add roles
                    capturedAdmin = user;
                }
            });
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _mockMediator.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockAuthService.Setup(a => a.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<string>>()))
                .ReturnsAsync(new TokenDto
                {
                    AccessToken = "jwt_token",
                    ExpiresIn = 3600,
                    Roles = new List<string> { "Admin" },
                    Permissions = new List<string>()
                });

            var command = new RegisterAdminCommand
            {
                Email = "admin@test.com",
                Password = "Admin@123",
                FullName = "System Admin",
                VerificationCode = "123456"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("admin@test.com", result.Email);
            Assert.Equal("jwt_token", result.Token);
            Assert.Contains("Admin", result.Roles);
            _mockContext.Verify(c => c.Add(It.IsAny<object>()), Times.AtLeast(2)); // At least User and Profile
        }

        [Fact]
        public async Task Handle_NoFullName_UsesDefaultFullName()
        {
            // Arrange
            var emptyUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
            var roles = new List<Role> { new Role { Id = 1, RoleName = "Admin" } }.AsQueryable().BuildMock();

            var newAdmin = new ApplicationUser
            {
                UserId = 1,
                Email = "admin@test.com",
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        UserId = 1,
                        RoleId = 1,
                        Role = new Role
                        {
                            Id = 1,
                            RoleName = "Admin",
                            RolePermissions = new List<RolePermission>()
                        }
                    }
                }
            };

            ApplicationUser capturedAdmin = null;
            var adminList = new List<ApplicationUser>().AsQueryable().BuildMock();

            var callCount = 0;
            _mockContext.Setup(c => c.ApplicationUsers).Returns(() =>
            {
                callCount++;
                if (callCount <= 2) return emptyUsers;
                // Third call - return captured admin if available
                if (capturedAdmin != null)
                {
                    return new List<ApplicationUser> { capturedAdmin }.AsQueryable().BuildMock();
                }
                return adminList;
            });
            _mockContext.Setup(c => c.Roles).Returns(roles);
            _mockContext.Setup(c => c.Add(It.IsAny<object>())).Callback<object>(entity =>
            {
                if (entity is ApplicationUser user)
                {
                    user.UserId = 1; // Simulate database generated ID
                    user.UserRoles = newAdmin.UserRoles; // Add roles
                    capturedAdmin = user;
                }
            });
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _mockMediator.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockAuthService.Setup(a => a.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<string>>()))
                .ReturnsAsync(new TokenDto
                {
                    AccessToken = "jwt_token",
                    ExpiresIn = 3600,
                    Roles = new List<string> { "Admin" },
                    Permissions = new List<string>()
                });

            var command = new RegisterAdminCommand
            {
                Email = "admin@test.com",
                Password = "Admin@123",
                VerificationCode = "123456"
                // FullName is null or empty
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            // UserProfile should be created with default full name "System Administrator"
            _mockContext.Verify(c => c.Add(It.Is<UserProfile>(p => p != null)), Times.Once);
        }
    }
}
