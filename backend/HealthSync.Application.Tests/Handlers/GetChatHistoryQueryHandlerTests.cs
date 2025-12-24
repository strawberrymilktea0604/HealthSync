using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers
{
    public class GetChatHistoryQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly GetChatHistoryQueryHandler _handler;

        public GetChatHistoryQueryHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new GetChatHistoryQueryHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoMessages()
        {
            // Arrange
            var messages = new List<ChatMessage>().AsQueryable().BuildMock();
            _mockContext.Setup(c => c.ChatMessages).Returns(messages);

            var query = new GetChatHistoryQuery
            {
                UserId = 1,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ReturnsMessagesForSpecificUser()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 1,
                    Role = "user",
                    Content = "Hello",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 1,
                    Role = "assistant",
                    Content = "Hi there!",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 2,
                    Role = "user",
                    Content = "Different user",
                    CreatedAt = DateTime.UtcNow
                }
            }.AsQueryable().BuildMock();

            _mockContext.Setup(c => c.ChatMessages).Returns(messages);

            var query = new GetChatHistoryQuery
            {
                UserId = 1,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, m => Assert.True(m.Role == "user" || m.Role == "assistant"));
            Assert.DoesNotContain(result, m => m.Content == "Different user");
        }

        [Fact]
        public async Task Handle_OrdersMessagesChronologically()
        {
            // Arrange
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 1,
                    Role = "assistant",
                    Content = "Response 1",
                    CreatedAt = DateTime.UtcNow.AddHours(-3)
                },
                new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 1,
                    Role = "user",
                    Content = "Question 2",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 1,
                    Role = "assistant",
                    Content = "Response 2",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                }
            }.AsQueryable().BuildMock();

            _mockContext.Setup(c => c.ChatMessages).Returns(messages);

            var query = new GetChatHistoryQuery
            {
                UserId = 1,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Response 1", result[0].Content);
            Assert.Equal("Question 2", result[1].Content);
            Assert.Equal("Response 2", result[2].Content);
        }

        [Fact]
        public async Task Handle_AppliesPagination()
        {
            // Arrange
            var messages = new List<ChatMessage>();
            for (int i = 0; i < 25; i++)
            {
                messages.Add(new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 1,
                    Role = i % 2 == 0 ? "user" : "assistant",
                    Content = $"Message {i}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }

            var mockMessages = messages.AsQueryable().BuildMock();
            _mockContext.Setup(c => c.ChatMessages).Returns(mockMessages);

            var query = new GetChatHistoryQuery
            {
                UserId = 1,
                PageNumber = 2,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPageSize()
        {
            // Arrange
            var messages = new List<ChatMessage>();
            for (int i = 0; i < 5; i++)
            {
                messages.Add(new ChatMessage
                {
                    ChatMessageId = Guid.NewGuid(),
                    UserId = 1,
                    Role = "user",
                    Content = $"Message {i}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }

            var mockMessages = messages.AsQueryable().BuildMock();
            _mockContext.Setup(c => c.ChatMessages).Returns(mockMessages);

            var query = new GetChatHistoryQuery
            {
                UserId = 1,
                PageNumber = 1,
                PageSize = 3
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task Handle_MapsAllProperties()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddHours(-1);

            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    ChatMessageId = messageId,
                    UserId = 1,
                    Role = "user",
                    Content = "Test message",
                    CreatedAt = createdAt
                }
            }.AsQueryable().BuildMock();

            _mockContext.Setup(c => c.ChatMessages).Returns(messages);

            var query = new GetChatHistoryQuery
            {
                UserId = 1,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(messageId, result[0].MessageId);
            Assert.Equal("user", result[0].Role);
            Assert.Equal("Test message", result[0].Content);
            Assert.Equal(createdAt, result[0].CreatedAt);
        }
    }
}
