// <copyright file="SessionCommentEndpointsTests.cs" company="RoundsApp">
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

public class SessionCommentEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SessionCommentEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetCommentsBySession_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var sessionId = Guid.NewGuid();
        var response = await this.client.GetAsync($"/api/session-comments/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommentsBySession_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        // Act
        var response = await this.client.GetAsync($"/api/session-comments/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<List<CommentResponse>>();
        comments.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateComment_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateCommentRequest
        {
            SessionId = sessionId,
            Content = "This is a test comment",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-comments/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CommentResponse>();
        result.Should().NotBeNull();
        result!.Content.Should().Be("This is a test comment");
    }

    [Fact]
    public async Task CreateComment_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateCommentRequest
        {
            SessionId = Guid.NewGuid(),
            Content = "Test comment",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-comments/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommentById_WithValidId_ReturnsComment()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateCommentRequest
        {
            SessionId = sessionId,
            Content = "Test comment",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-comments/", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/session-comments/{createdComment!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommentResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdComment.Id);
    }

    [Fact]
    public async Task GetCommentById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/session-comments/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateComment_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateCommentRequest
        {
            SessionId = sessionId,
            Content = "Original content",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-comments/", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        var updateRequest = new UpdateCommentRequest
        {
            Content = "Updated content",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-comments/{createdComment!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CommentResponse>();
        result.Should().NotBeNull();
        result!.Content.Should().Be("Updated content");
    }

    [Fact]
    public async Task UpdateComment_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateCommentRequest
        {
            SessionId = sessionId,
            Content = "Original content",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-comments/", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateCommentRequest
        {
            Content = "Updated content",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-comments/{createdComment!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateCommentRequest
        {
            SessionId = sessionId,
            Content = "Test comment",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-comments/", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/session-comments/{createdComment!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteComment_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateCommentRequest
        {
            SessionId = sessionId,
            Content = "Test comment",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-comments/", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-comments/{createdComment!.Id}");

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
