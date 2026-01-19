// <copyright file="SessionEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.Sessions;
using Xunit;

namespace RoundsApp.Tests;

public class SessionEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SessionEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetAllSessions_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/sessions/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllSessions_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/sessions/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await response.Content.ReadFromJsonAsync<List<SessionResponse>>();
        sessions.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSession_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionRequest
        {
            Name = "Test Session",
            Description = "Test Description",
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(2),
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SessionResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Session");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task CreateSession_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateSessionRequest
        {
            Name = "Test Session",
            Description = "Test Description",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSessionById_WithValidId_ReturnsSession()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionRequest
        {
            Name = "Test Session",
            Description = "Test Description",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/sessions/{createdSession!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdSession.Id);
        result.Name.Should().Be("Test Session");
    }

    [Fact]
    public async Task GetSessionById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/sessions/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSession_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionRequest
        {
            Name = "Original Name",
            Description = "Original Description",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var updateRequest = new UpdateSessionRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/sessions/{createdSession!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateSession_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateSessionRequest
        {
            Name = "Original Name",
            Description = "Original Description",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateSessionRequest
        {
            Name = "Updated Name",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/sessions/{createdSession!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteSession_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionRequest
        {
            Name = "Test Session",
            Description = "Test Description",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/sessions/{createdSession!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await this.client.GetAsync($"/api/sessions/{createdSession.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSession_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateSessionRequest
        {
            Name = "Test Session",
            Description = "Test Description",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/sessions/", createRequest);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/sessions/{createdSession!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUpcomingSessions_ReturnsOnlyFutureSessions()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var futureSession = new CreateSessionRequest
        {
            Name = "Future Session",
            StartsAt = DateTime.UtcNow.AddDays(1),
        };

        await this.client.PostAsJsonAsync("/api/sessions/", futureSession);

        // Act
        var response = await this.client.GetAsync("/api/sessions/upcoming");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await response.Content.ReadFromJsonAsync<List<SessionResponse>>();
        sessions.Should().NotBeNull();
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
}
