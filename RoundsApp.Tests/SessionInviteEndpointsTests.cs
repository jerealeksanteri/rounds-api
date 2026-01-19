// <copyright file="SessionInviteEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.Sessions;
using Xunit;

namespace RoundsApp.Tests;

public class SessionInviteEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SessionInviteEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetInvitesBySession_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var sessionId = Guid.NewGuid();
        var response = await this.client.GetAsync($"/api/session-invites/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInvitesBySession_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        // Act
        var response = await this.client.GetAsync($"/api/session-invites/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invites = await response.Content.ReadFromJsonAsync<List<SessionInviteResponse>>();
        invites.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMyInvites_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/session-invites/my-invites");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invites = await response.Content.ReadFromJsonAsync<List<SessionInviteResponse>>();
        invites.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPendingInvites_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/session-invites/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invites = await response.Content.ReadFromJsonAsync<List<SessionInviteResponse>>();
        invites.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateInvite_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SessionInviteResponse>();
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(sessionId);
        result.UserId.Should().Be(userId);
        result.Status.Should().Be("pending");
    }

    [Fact]
    public async Task CreateInvite_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInviteById_WithValidId_ReturnsInvite()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);
        var createdInvite = await createResponse.Content.ReadFromJsonAsync<SessionInviteResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/session-invites/{createdInvite!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionInviteResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdInvite.Id);
    }

    [Fact]
    public async Task GetInviteById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/session-invites/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateInvite_ByInvitee_ReturnsOk()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var (token2, userId2) = await this.RegisterAndLoginWithIdAsync();

        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = sessionId,
            UserId = userId2,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);
        var createdInvite = await createResponse.Content.ReadFromJsonAsync<SessionInviteResponse>();

        // Login as the invitee
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateSessionInviteRequest
        {
            Status = "accepted",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-invites/{createdInvite!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionInviteResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("accepted");
    }

    [Fact]
    public async Task UpdateInvite_ByOtherUser_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);
        var createdInvite = await createResponse.Content.ReadFromJsonAsync<SessionInviteResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateSessionInviteRequest
        {
            Status = "accepted",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-invites/{createdInvite!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteInvite_ByCreator_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);
        var createdInvite = await createResponse.Content.ReadFromJsonAsync<SessionInviteResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/session-invites/{createdInvite!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteInvite_ByInvitee_ReturnsNoContent()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var (token2, userId2) = await this.RegisterAndLoginWithIdAsync();

        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = sessionId,
            UserId = userId2,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);
        var createdInvite = await createResponse.Content.ReadFromJsonAsync<SessionInviteResponse>();

        // Login as the invitee
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-invites/{createdInvite!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteInvite_ByUnrelatedUser_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateSessionInviteRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-invites/", createRequest);
        var createdInvite = await createResponse.Content.ReadFromJsonAsync<SessionInviteResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-invites/{createdInvite!.Id}");

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

    private async Task<Guid> CreateSessionAsync()
    {
        var createRequest = new CreateSessionRequest
        {
            Name = $"Test Session {Guid.NewGuid().ToString()[..8]}",
            Description = "Test Description",
        };

        var response = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        return session!.Id;
    }
}
