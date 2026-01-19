// <copyright file="SessionImageEndpointsTests.cs" company="RoundsApp">
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

public class SessionImageEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SessionImageEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetImagesBySession_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var sessionId = Guid.NewGuid();
        var response = await this.client.GetAsync($"/api/session-images/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetImagesBySession_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        // Act
        var response = await this.client.GetAsync($"/api/session-images/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var images = await response.Content.ReadFromJsonAsync<List<SessionImageResponse>>();
        images.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateImage_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionImageRequest
        {
            SessionId = sessionId,
            Url = "https://example.com/image.jpg",
            Caption = "Test caption",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-images/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SessionImageResponse>();
        result.Should().NotBeNull();
        result!.Url.Should().Be("https://example.com/image.jpg");
        result.Caption.Should().Be("Test caption");
    }

    [Fact]
    public async Task CreateImage_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateSessionImageRequest
        {
            SessionId = Guid.NewGuid(),
            Url = "https://example.com/image.jpg",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-images/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetImageById_WithValidId_ReturnsImage()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionImageRequest
        {
            SessionId = sessionId,
            Url = "https://example.com/image.jpg",
            Caption = "Test caption",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-images/", createRequest);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<SessionImageResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/session-images/{createdImage!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionImageResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdImage.Id);
    }

    [Fact]
    public async Task GetImageById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/session-images/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateImage_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionImageRequest
        {
            SessionId = sessionId,
            Url = "https://example.com/image.jpg",
            Caption = "Original caption",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-images/", createRequest);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<SessionImageResponse>();

        var updateRequest = new UpdateSessionImageRequest
        {
            Caption = "Updated caption",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-images/{createdImage!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionImageResponse>();
        result.Should().NotBeNull();
        result!.Caption.Should().Be("Updated caption");
    }

    [Fact]
    public async Task UpdateImage_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionImageRequest
        {
            SessionId = sessionId,
            Url = "https://example.com/image.jpg",
            Caption = "Original caption",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-images/", createRequest);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<SessionImageResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateSessionImageRequest
        {
            Caption = "Updated caption",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-images/{createdImage!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteImage_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionImageRequest
        {
            SessionId = sessionId,
            Url = "https://example.com/image.jpg",
            Caption = "Test caption",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-images/", createRequest);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<SessionImageResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/session-images/{createdImage!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteImage_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionImageRequest
        {
            SessionId = sessionId,
            Url = "https://example.com/image.jpg",
            Caption = "Test caption",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-images/", createRequest);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<SessionImageResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-images/{createdImage!.Id}");

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
