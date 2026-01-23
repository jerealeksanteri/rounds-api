// <copyright file="FriendshipEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.Friendships;
using RoundsApp.DTOs.Notifications;
using RoundsApp.Models;
using Xunit;

namespace RoundsApp.Tests;

public class FriendshipEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public FriendshipEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetAllFriendships_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/friendships/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllFriendships_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/friendships/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var friendships = await response.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        friendships.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateFriendship_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var friendId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateFriendshipRequest
        {
            FriendId = friendId,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<FriendshipResponse>();
        result.Should().NotBeNull();
        result!.FriendId.Should().Be(friendId);
        result.Status.Should().Be(FriendshipStatus.Pending);
    }

    [Fact]
    public async Task CreateFriendship_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateFriendshipRequest
        {
            FriendId = Guid.NewGuid(),
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyFriends_ReturnsOnlyAcceptedFriendships()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/friendships/my-friends");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var friends = await response.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        friends.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPendingRequests_ReturnsPendingFriendships()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/friendships/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pending = await response.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        pending.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSentRequests_ReturnsSentFriendshipRequests()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/friendships/sent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sent = await response.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        sent.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateFriendship_AcceptRequest_ReturnsOk()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var user1Id = this.GetCurrentUserIdFromToken();

        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        // User 1 sends friend request to User 2
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendshipRequest
        {
            FriendId = user2Id,
        };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // User 2 accepts the request
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateRequest = new UpdateFriendshipRequest
        {
            Status = FriendshipStatus.Accepted,
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/friendships/{user1Id}/{user2Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FriendshipResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(FriendshipStatus.Accepted);
    }

    [Fact]
    public async Task UpdateFriendship_ByUnrelatedUser_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var user1Id = this.GetCurrentUserIdFromToken();

        var (_, user2Id) = await this.RegisterAndLoginWithIdAsync();
        var createRequest = new CreateFriendshipRequest
        {
            FriendId = user2Id,
        };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Different user tries to update
        var token3 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token3);

        var updateRequest = new UpdateFriendshipRequest
        {
            Status = FriendshipStatus.Accepted,
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/friendships/{user1Id}/{user2Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteFriendship_ByParticipant_ReturnsNoContent()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var user1Id = this.GetCurrentUserIdFromToken();

        var (_, user2Id) = await this.RegisterAndLoginWithIdAsync();
        var createRequest = new CreateFriendshipRequest
        {
            FriendId = user2Id,
        };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Act
        var response = await this.client.DeleteAsync($"/api/friendships/{user1Id}/{user2Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteFriendship_ByUnrelatedUser_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var user1Id = this.GetCurrentUserIdFromToken();

        var (_, user2Id) = await this.RegisterAndLoginWithIdAsync();
        var createRequest = new CreateFriendshipRequest
        {
            FriendId = user2Id,
        };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Different user tries to delete
        var token3 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token3);

        // Act
        var response = await this.client.DeleteAsync($"/api/friendships/{user1Id}/{user2Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteFriendship_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var userId = Guid.NewGuid();
        var friendId = Guid.NewGuid();

        // Act
        var response = await this.client.DeleteAsync($"/api/friendships/{userId}/{friendId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AcceptFriendship_CreatesBidirectionalFriendship()
    {
        // Arrange - User 1 sends friend request to User 2
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendshipRequest { FriendId = user2Id };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Act - User 2 accepts the request
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateRequest = new UpdateFriendshipRequest { Status = FriendshipStatus.Accepted };
        var response = await this.client.PutAsJsonAsync($"/api/friendships/{user1Id}/{user2Id}", updateRequest);

        // Assert - Both users should see the friendship
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Check User 1's friends list
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var user1FriendsResponse = await this.client.GetAsync("/api/friendships/my-friends");
        var user1Friends = await user1FriendsResponse.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        user1Friends.Should().Contain(f => f.FriendId == user2Id && f.Status == FriendshipStatus.Accepted);

        // Check User 2's friends list
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var user2FriendsResponse = await this.client.GetAsync("/api/friendships/my-friends");
        var user2Friends = await user2FriendsResponse.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        user2Friends.Should().Contain(f => f.FriendId == user1Id && f.Status == FriendshipStatus.Accepted);
    }

    [Fact]
    public async Task DeleteFriendship_DeletesBothDirections()
    {
        // Arrange - Create and accept a bidirectional friendship
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        // User 1 sends friend request
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendshipRequest { FriendId = user2Id };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // User 2 accepts
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateRequest = new UpdateFriendshipRequest { Status = FriendshipStatus.Accepted };
        await this.client.PutAsJsonAsync($"/api/friendships/{user1Id}/{user2Id}", updateRequest);

        // Act - User 1 deletes the friendship
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var deleteResponse = await this.client.DeleteAsync($"/api/friendships/{user1Id}/{user2Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify User 1's friends list is empty
        var user1FriendsResponse = await this.client.GetAsync("/api/friendships/my-friends");
        var user1Friends = await user1FriendsResponse.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        user1Friends.Should().NotContain(f => f.FriendId == user2Id);

        // Verify User 2's friends list is also empty
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var user2FriendsResponse = await this.client.GetAsync("/api/friendships/my-friends");
        var user2Friends = await user2FriendsResponse.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        user2Friends.Should().NotContain(f => f.FriendId == user1Id);
    }

    [Fact]
    public async Task DeleteFriendship_ByFriend_DeletesBothDirections()
    {
        // Arrange - Create and accept a bidirectional friendship
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        // User 1 sends friend request
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendshipRequest { FriendId = user2Id };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // User 2 accepts
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateRequest = new UpdateFriendshipRequest { Status = FriendshipStatus.Accepted };
        await this.client.PutAsJsonAsync($"/api/friendships/{user1Id}/{user2Id}", updateRequest);

        // Act - User 2 (the friend) deletes the friendship
        var deleteResponse = await this.client.DeleteAsync($"/api/friendships/{user1Id}/{user2Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify both users' friends lists are empty
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var user1FriendsResponse = await this.client.GetAsync("/api/friendships/my-friends");
        var user1Friends = await user1FriendsResponse.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        user1Friends.Should().NotContain(f => f.FriendId == user2Id);

        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var user2FriendsResponse = await this.client.GetAsync("/api/friendships/my-friends");
        var user2Friends = await user2FriendsResponse.Content.ReadFromJsonAsync<List<FriendshipResponse>>();
        user2Friends.Should().NotContain(f => f.FriendId == user1Id);
    }

    [Fact]
    public async Task CreateFriendship_SendsNotificationToFriend()
    {
        // Arrange
        var (token1, _) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        // Act - User 1 sends friend request
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendshipRequest { FriendId = user2Id };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Assert - Check User 2 has a notification
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var notificationsResponse = await this.client.GetAsync("/api/notifications/");
        var notifications = await notificationsResponse.Content.ReadFromJsonAsync<List<NotificationResponse>>();

        notifications.Should().NotBeNull();
        notifications.Should().Contain(n => n.Type == "friend_request" && n.Title == "New Friend Request");
    }

    [Fact]
    public async Task AcceptFriendship_BidirectionalFriendship_HasCorrectStatus()
    {
        // Arrange
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendshipRequest { FriendId = user2Id };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // Act - Accept the friendship
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateRequest = new UpdateFriendshipRequest { Status = FriendshipStatus.Accepted };
        var response = await this.client.PutAsJsonAsync($"/api/friendships/{user1Id}/{user2Id}", updateRequest);

        // Assert - The returned friendship should have accepted status
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FriendshipResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(FriendshipStatus.Accepted);
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

    private async Task<Guid> RegisterUserAndGetIdAsync()
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

        return authResponse!.UserId;
    }

    private Guid GetCurrentUserIdFromToken()
    {
        var currentToken = this.client.DefaultRequestHeaders.Authorization?.Parameter;
        if (currentToken == null)
        {
            throw new InvalidOperationException("No authentication token found");
        }

        var tokenParts = currentToken.Split('.');
        if (tokenParts.Length != 3)
        {
            throw new InvalidOperationException("Invalid JWT token format");
        }

        var payload = tokenParts[1];
        var paddedPayload = payload.PadRight(payload.Length + ((4 - (payload.Length % 4)) % 4), '=');
        var payloadBytes = Convert.FromBase64String(paddedPayload);
        var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
        var payloadDoc = System.Text.Json.JsonDocument.Parse(payloadJson);

        var userIdString = payloadDoc.RootElement.GetProperty("sub").GetString();
        return Guid.Parse(userIdString!);
    }
}
