// <copyright file="NotificationEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.Notifications;
using Xunit;

namespace RoundsApp.Tests;

public class NotificationEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public NotificationEndpointsTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("JwtSettings:SecretKey", "test-secret-key-for-testing-purposes-only-minimum-32-chars");
            builder.UseSetting("JwtSettings:Issuer", "RoundsAppTest");
            builder.UseSetting("JwtSettings:Audience", "RoundsAppTestAudience");
            builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Port=5432;Database=roundsdb_test;Username=roundsuser;Password=roundspassword");

            builder.UseEnvironment("Test");
        });

        this.client = customFactory.CreateClient();
    }

    [Fact]
    public async Task GetMyNotifications_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/notifications/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyNotifications_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/notifications/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        notifications.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateNotification_WithValidData_ReturnsCreated()
    {
        // Arrange
        var (token, userId) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId,
            Type = "info",
            Title = "Test Notification",
            Message = "This is a test notification",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/notifications/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<NotificationResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Notification");
        result.Read.Should().BeFalse();
    }

    [Fact]
    public async Task GetUnreadNotifications_ReturnsOnlyUnread()
    {
        // Arrange
        var (token, userId) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId,
            Type = "info",
            Title = "Unread Notification",
            Message = "This should appear in unread",
        };

        await this.client.PostAsJsonAsync("/api/notifications/", createRequest);

        // Act
        var response = await this.client.GetAsync("/api/notifications/unread");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        notifications.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNotificationById_WithValidId_ReturnsNotification()
    {
        // Arrange
        var (token, userId) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId,
            Type = "info",
            Title = "Test Notification",
            Message = "Test message",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/notifications/", createRequest);
        var createdNotification = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/notifications/{createdNotification!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<NotificationResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdNotification.Id);
    }

    [Fact]
    public async Task GetNotificationById_ForOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (token1, userId1) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId1,
            Type = "info",
            Title = "Test Notification",
            Message = "Test message",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/notifications/", createRequest);
        var createdNotification = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.GetAsync($"/api/notifications/{createdNotification!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MarkAsRead_WithValidId_ReturnsOk()
    {
        // Arrange
        var (token, userId) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId,
            Type = "info",
            Title = "Test Notification",
            Message = "Test message",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/notifications/", createRequest);
        var createdNotification = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();

        // Act
        var response = await this.client.PutAsync($"/api/notifications/{createdNotification!.Id}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MarkAsRead_ForOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (token1, userId1) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId1,
            Type = "info",
            Title = "Test Notification",
            Message = "Test message",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/notifications/", createRequest);
        var createdNotification = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.PutAsync($"/api/notifications/{createdNotification!.Id}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOk()
    {
        // Arrange
        var (token, userId) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a couple of notifications
        var createRequest = new CreateNotificationRequest
        {
            UserId = userId,
            Type = "info",
            Title = "Test Notification",
            Message = "Test message",
        };

        await this.client.PostAsJsonAsync("/api/notifications/", createRequest);
        await this.client.PostAsJsonAsync("/api/notifications/", createRequest);

        // Act
        var response = await this.client.PutAsync("/api/notifications/read-all", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteNotification_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var (token, userId) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId,
            Type = "info",
            Title = "Test Notification",
            Message = "Test message",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/notifications/", createRequest);
        var createdNotification = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/notifications/{createdNotification!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteNotification_ForOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (token1, userId1) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateNotificationRequest
        {
            UserId = userId1,
            Type = "info",
            Title = "Test Notification",
            Message = "Test message",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/notifications/", createRequest);
        var createdNotification = await createResponse.Content.ReadFromJsonAsync<NotificationResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/notifications/{createdNotification!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";
        var userName = $"testuser{Guid.NewGuid().ToString()[..8]}";

        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User",
            UserName = userName,
        };

        await this.client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password,
        };

        var loginResponse = await this.client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        return authResponse!.Token;
    }

    private async Task<(string Token, Guid UserId)> RegisterAndLoginWithIdAsync()
    {
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";
        var userName = $"testuser{Guid.NewGuid().ToString()[..8]}";

        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User",
            UserName = userName,
        };

        await this.client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password,
        };

        var loginResponse = await this.client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        return (authResponse!.Token, authResponse.UserId);
    }
}
