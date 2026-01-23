// <copyright file="FriendGroupEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.FriendGroups;
using RoundsApp.DTOs.Friendships;
using RoundsApp.DTOs.Sessions;
using RoundsApp.Models;
using Xunit;

namespace RoundsApp.Tests;

public class FriendGroupEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public FriendGroupEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetMyGroups_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/friend-groups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyGroups_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/friend-groups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<FriendGroupResponse>>();
        groups.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateGroup_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateFriendGroupRequest
        {
            Name = "Test Group",
            Description = "A test group",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<FriendGroupResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Group");
        result.Description.Should().Be("A test group");
        result.MemberCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateGroup_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateFriendGroupRequest
        {
            Name = "Test Group",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateGroup_WithInitialMembers_AddsMembers()
    {
        // Arrange - Create two users who are friends
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        // Make them friends
        await this.CreateAcceptedFriendshipAsync(token1, user1Id, token2, user2Id);

        // Act - User 1 creates a group with User 2 as initial member
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendGroupRequest
        {
            Name = "Friends Group",
            InitialMemberIds = new List<Guid> { user2Id },
        };
        var response = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<FriendGroupResponse>();
        result.Should().NotBeNull();
        result!.MemberCount.Should().Be(1);
        result.Members.Should().Contain(m => m.UserId == user2Id);
    }

    [Fact]
    public async Task CreateGroup_WithNonFriendAsInitialMember_ReturnsBadRequest()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var nonFriendId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateFriendGroupRequest
        {
            Name = "Test Group",
            InitialMemberIds = new List<Guid> { nonFriendId },
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetGroupById_ReturnsGroup()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateFriendGroupRequest { Name = "My Group" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/friend-groups/{createdGroup!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FriendGroupResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdGroup.Id);
        result.Name.Should().Be("My Group");
    }

    [Fact]
    public async Task GetGroupById_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync($"/api/friend-groups/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupById_ByNonOwner_ReturnsForbidden()
    {
        // Arrange - User 1 creates a group
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateFriendGroupRequest { Name = "Private Group" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // User 2 tries to access it
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.GetAsync($"/api/friend-groups/{createdGroup!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateGroup_UpdatesSuccessfully()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateFriendGroupRequest { Name = "Original Name" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        var updateRequest = new UpdateFriendGroupRequest
        {
            Name = "Updated Name",
            Description = "New description",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/friend-groups/{createdGroup!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FriendGroupResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("New description");
    }

    [Fact]
    public async Task UpdateGroup_ByNonOwner_ReturnsForbidden()
    {
        // Arrange - User 1 creates a group
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateFriendGroupRequest { Name = "My Group" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // User 2 tries to update it
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateFriendGroupRequest { Name = "Hacked Name" };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/friend-groups/{createdGroup!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteGroup_DeletesSuccessfully()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateFriendGroupRequest { Name = "To Delete" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/friend-groups/{createdGroup!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await this.client.GetAsync($"/api/friend-groups/{createdGroup.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGroup_ByNonOwner_ReturnsForbidden()
    {
        // Arrange - User 1 creates a group
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateFriendGroupRequest { Name = "Protected Group" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // User 2 tries to delete it
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/friend-groups/{createdGroup!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddMembers_AddsMembersSuccessfully()
    {
        // Arrange - Create friends
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        await this.CreateAcceptedFriendshipAsync(token1, user1Id, token2, user2Id);

        // User 1 creates a group
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendGroupRequest { Name = "My Group" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        var addMembersRequest = new AddGroupMembersRequest
        {
            UserIds = new List<Guid> { user2Id },
        };

        // Act
        var response = await this.client.PostAsJsonAsync($"/api/friend-groups/{createdGroup!.Id}/members", addMembersRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FriendGroupResponse>();
        result.Should().NotBeNull();
        result!.MemberCount.Should().Be(1);
        result.Members.Should().Contain(m => m.UserId == user2Id);
    }

    [Fact]
    public async Task AddMembers_NonFriends_ReturnsBadRequest()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateFriendGroupRequest { Name = "My Group" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        var nonFriendId = await this.RegisterUserAndGetIdAsync();

        var addMembersRequest = new AddGroupMembersRequest
        {
            UserIds = new List<Guid> { nonFriendId },
        };

        // Act
        var response = await this.client.PostAsJsonAsync($"/api/friend-groups/{createdGroup!.Id}/members", addMembersRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddMembers_AlreadyMembers_ReturnsBadRequest()
    {
        // Arrange - Create friends and add to group
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        await this.CreateAcceptedFriendshipAsync(token1, user1Id, token2, user2Id);

        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendGroupRequest
        {
            Name = "My Group",
            InitialMemberIds = new List<Guid> { user2Id },
        };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // Try to add the same member again
        var addMembersRequest = new AddGroupMembersRequest
        {
            UserIds = new List<Guid> { user2Id },
        };

        // Act
        var response = await this.client.PostAsJsonAsync($"/api/friend-groups/{createdGroup!.Id}/members", addMembersRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveMember_RemovesSuccessfully()
    {
        // Arrange - Create friends and add to group
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        await this.CreateAcceptedFriendshipAsync(token1, user1Id, token2, user2Id);

        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendGroupRequest
        {
            Name = "My Group",
            InitialMemberIds = new List<Guid> { user2Id },
        };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/friend-groups/{createdGroup!.Id}/members/{user2Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify member is removed
        var getResponse = await this.client.GetAsync($"/api/friend-groups/{createdGroup.Id}");
        var group = await getResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();
        group!.MemberCount.Should().Be(0);
    }

    [Fact]
    public async Task RemoveMember_NonMember_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateFriendGroupRequest { Name = "My Group" };
        var createResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createRequest);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/friend-groups/{createdGroup!.Id}/members/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BulkInviteToSession_InvitesAllMembers()
    {
        // Arrange - Create friends and add to group
        var (token1, user1Id) = await this.RegisterAndLoginWithIdAsync();
        var (token2, user2Id) = await this.RegisterAndLoginWithIdAsync();

        await this.CreateAcceptedFriendshipAsync(token1, user1Id, token2, user2Id);

        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        // Create a group with a member
        var createGroupRequest = new CreateFriendGroupRequest
        {
            Name = "Drinking Buddies",
            InitialMemberIds = new List<Guid> { user2Id },
        };
        var groupResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createGroupRequest);
        var group = await groupResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        // Create a session
        var createSessionRequest = new CreateSessionRequest
        {
            Name = "Friday Night",
        };
        var sessionResponse = await this.client.PostAsJsonAsync("/api/sessions", createSessionRequest);
        var session = await sessionResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var bulkInviteRequest = new BulkInviteToSessionRequest
        {
            SessionId = session!.Id,
        };

        // Act
        var response = await this.client.PostAsJsonAsync($"/api/friend-groups/{group!.Id}/invite-to-session", bulkInviteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BulkInviteResponse>();
        result.Should().NotBeNull();
        result!.InvitesSent.Should().Be(1);
    }

    [Fact]
    public async Task BulkInviteToSession_NonExistentSession_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createGroupRequest = new CreateFriendGroupRequest { Name = "My Group" };
        var groupResponse = await this.client.PostAsJsonAsync("/api/friend-groups", createGroupRequest);
        var group = await groupResponse.Content.ReadFromJsonAsync<FriendGroupResponse>();

        var bulkInviteRequest = new BulkInviteToSessionRequest
        {
            SessionId = Guid.NewGuid(),
        };

        // Act
        var response = await this.client.PostAsJsonAsync($"/api/friend-groups/{group!.Id}/invite-to-session", bulkInviteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

    private async Task CreateAcceptedFriendshipAsync(string token1, Guid user1Id, string token2, Guid user2Id)
    {
        // User 1 sends friend request
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var createRequest = new CreateFriendshipRequest { FriendId = user2Id };
        await this.client.PostAsJsonAsync("/api/friendships/", createRequest);

        // User 2 accepts
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateRequest = new UpdateFriendshipRequest { Status = FriendshipStatus.Accepted };
        await this.client.PutAsJsonAsync($"/api/friendships/{user1Id}/{user2Id}", updateRequest);
    }

    private class BulkInviteResponse
    {
#pragma warning disable S3459, S1144 // Needed for JSON deserialization
        public int InvitesSent { get; set; }
#pragma warning restore S3459, S1144

        public string GroupName { get; set; } = string.Empty;

        public string SessionName { get; set; } = string.Empty;
    }
}
