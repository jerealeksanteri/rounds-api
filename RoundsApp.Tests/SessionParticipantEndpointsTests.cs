// <copyright file="SessionParticipantEndpointsTests.cs" company="RoundsApp">
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

public class SessionParticipantEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SessionParticipantEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetParticipantsBySession_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var sessionId = Guid.NewGuid();
        var response = await this.client.GetAsync($"/api/session-participants/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetParticipantsBySession_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        // Act
        var response = await this.client.GetAsync($"/api/session-participants/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var participants = await response.Content.ReadFromJsonAsync<List<ParticipantResponse>>();
        participants.Should().NotBeNull();
    }

    [Fact]
    public async Task GetParticipantsByUser_WithAuth_ReturnsOk()
    {
        // Arrange
        var (token, userId) = await this.RegisterAndLoginWithIdAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync($"/api/session-participants/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var participants = await response.Content.ReadFromJsonAsync<List<ParticipantResponse>>();
        participants.Should().NotBeNull();
    }

    [Fact]
    public async Task AddParticipant_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateParticipantRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-participants/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ParticipantResponse>();
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(sessionId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task AddParticipant_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateParticipantRequest
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-participants/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddParticipant_WithInvalidSession_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateParticipantRequest
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-participants/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveParticipant_BySessionOwner_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateParticipantRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-participants/", createRequest);
        var createdParticipant = await createResponse.Content.ReadFromJsonAsync<ParticipantResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/session-participants/{createdParticipant!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveParticipant_BySelf_ReturnsNoContent()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var (token2, userId2) = await this.RegisterAndLoginWithIdAsync();

        // Add participant with first user (session owner)
        var createRequest = new CreateParticipantRequest
        {
            SessionId = sessionId,
            UserId = userId2,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-participants/", createRequest);
        var createdParticipant = await createResponse.Content.ReadFromJsonAsync<ParticipantResponse>();

        // Login as the participant
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-participants/{createdParticipant!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveParticipant_ByUnrelatedUser_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();
        var userId = await this.RegisterUserAndGetIdAsync();

        var createRequest = new CreateParticipantRequest
        {
            SessionId = sessionId,
            UserId = userId,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-participants/", createRequest);
        var createdParticipant = await createResponse.Content.ReadFromJsonAsync<ParticipantResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-participants/{createdParticipant!.Id}");

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
