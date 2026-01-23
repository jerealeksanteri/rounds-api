// <copyright file="NotificationServiceTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using RoundsApp.DTOs.Notifications;
using RoundsApp.Hubs;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;
using RoundsApp.Services;
using Xunit;

namespace RoundsApp.Tests;

public class NotificationServiceTests
{
    private readonly Mock<IHubContext<NotificationHub>> mockHubContext;
    private readonly Mock<INotificationRepository> mockNotificationRepository;
    private readonly Mock<IHubClients> mockClients;
    private readonly Mock<IClientProxy> mockClientProxy;
    private readonly NotificationService notificationService;

    public NotificationServiceTests()
    {
        this.mockHubContext = new Mock<IHubContext<NotificationHub>>();
        this.mockNotificationRepository = new Mock<INotificationRepository>();
        this.mockClients = new Mock<IHubClients>();
        this.mockClientProxy = new Mock<IClientProxy>();

        this.mockHubContext.Setup(x => x.Clients).Returns(this.mockClients.Object);
        this.mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(this.mockClientProxy.Object);

        this.notificationService = new NotificationService(
            this.mockHubContext.Object,
            this.mockNotificationRepository.Object);
    }

    [Fact]
    public async Task SendNotificationAsync_SendsToCorrectUserGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new NotificationResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = "test",
            Title = "Test Title",
            Message = "Test Message",
            Read = false,
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        await this.notificationService.SendNotificationAsync(userId, notification);

        // Assert
        this.mockClients.Verify(x => x.Group(userId.ToString()), Times.Once);
        this.mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args.Length == 1 && args[0] == notification),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SendNotificationToMultipleAsync_SendsToAllUsers()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var notification = new NotificationResponse
        {
            Id = Guid.NewGuid(),
            UserId = userIds[0],
            Type = "test",
            Title = "Test Title",
            Message = "Test Message",
            Read = false,
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        await this.notificationService.SendNotificationToMultipleAsync(userIds, notification);

        // Assert
        foreach (var userId in userIds)
        {
            this.mockClients.Verify(x => x.Group(userId.ToString()), Times.Once);
        }
    }

    [Fact]
    public async Task CreateAndSendAsync_CreatesNotificationInDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = "friend_request";
        var title = "New Friend Request";
        var body = "User X sent you a friend request";

        this.mockNotificationRepository
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);

        // Act
        await this.notificationService.CreateAndSendAsync(userId, type, title, body);

        // Assert
        this.mockNotificationRepository.Verify(
            x => x.CreateAsync(It.Is<Notification>(n =>
                n.UserId == userId &&
                n.Type == type &&
                n.Title == title &&
                n.Message == body &&
                !n.Read)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAndSendAsync_SendsNotificationViaSignalR()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = "session_invite";
        var title = "Session Invite";
        var body = "You have been invited to a session";

        this.mockNotificationRepository
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);

        // Act
        await this.notificationService.CreateAndSendAsync(userId, type, title, body);

        // Assert
        this.mockClients.Verify(x => x.Group(userId.ToString()), Times.Once);
        this.mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args.Length == 1),
                default),
            Times.Once);
    }

    [Fact]
    public async Task CreateAndSendAsync_WithMetadata_IncludesMetadataInNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = "friend_request";
        var title = "New Friend Request";
        var body = "User X sent you a friend request";
        var metadata = "{\"userId\":\"123\"}";

        this.mockNotificationRepository
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);

        // Act
        await this.notificationService.CreateAndSendAsync(userId, type, title, body, metadata);

        // Assert
        this.mockNotificationRepository.Verify(
            x => x.CreateAsync(It.Is<Notification>(n =>
                n.Metadata == metadata)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAndSendAsync_SetsCorrectTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        this.mockNotificationRepository
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);

        // Act
        await this.notificationService.CreateAndSendAsync(userId, "test", "Title", "Body");

        var afterCreation = DateTime.UtcNow;

        // Assert
        this.mockNotificationRepository.Verify(
            x => x.CreateAsync(It.Is<Notification>(n =>
                n.CreatedAt >= beforeCreation &&
                n.CreatedAt <= afterCreation)),
            Times.Once);
    }

    [Fact]
    public async Task SendNotificationToMultipleAsync_WithEmptyList_DoesNotSendAnything()
    {
        // Arrange
        var userIds = new List<Guid>();
        var notification = new NotificationResponse
        {
            Id = Guid.NewGuid(),
            Type = "test",
            Title = "Test",
            Message = "Test",
        };

        // Act
        await this.notificationService.SendNotificationToMultipleAsync(userIds, notification);

        // Assert
        this.mockClients.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAndSendAsync_GeneratesUniqueNotificationId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Guid capturedId = Guid.Empty;

        this.mockNotificationRepository
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => capturedId = n.Id)
            .ReturnsAsync((Notification n) => n);

        // Act
        await this.notificationService.CreateAndSendAsync(userId, "test", "Title", "Body");

        // Assert
        capturedId.Should().NotBe(Guid.Empty);
    }
}
