// <copyright file="SessionTagEndpointsTests.cs" company="RoundsApp">
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

public class SessionTagEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SessionTagEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetTagsBySession_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var sessionId = Guid.NewGuid();
        var response = await this.client.GetAsync($"/api/session-tags/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTagsBySession_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        // Act
        var response = await this.client.GetAsync($"/api/session-tags/session/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tags = await response.Content.ReadFromJsonAsync<List<SessionTagResponse>>();
        tags.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTagsByName_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionTagRequest
        {
            SessionId = sessionId,
            Tag = "party",
        };

        await this.client.PostAsJsonAsync("/api/session-tags/", createRequest);

        // Act
        var response = await this.client.GetAsync("/api/session-tags/search?name=party");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tags = await response.Content.ReadFromJsonAsync<List<SessionTagResponse>>();
        tags.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTag_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionTagRequest
        {
            SessionId = sessionId,
            Tag = "birthday",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-tags/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SessionTagResponse>();
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(sessionId);
        result.Tag.Should().Be("birthday");
    }

    [Fact]
    public async Task CreateTag_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateSessionTagRequest
        {
            SessionId = Guid.NewGuid(),
            Tag = "test",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-tags/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTagById_WithValidId_ReturnsTag()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionTagRequest
        {
            SessionId = sessionId,
            Tag = "celebration",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-tags/", createRequest);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<SessionTagResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/session-tags/{createdTag!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionTagResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdTag.Id);
        result.Tag.Should().Be("celebration");
    }

    [Fact]
    public async Task GetTagById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/session-tags/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTag_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionTagRequest
        {
            SessionId = sessionId,
            Tag = "temporary",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-tags/", createRequest);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<SessionTagResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/session-tags/{createdTag!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTag_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var sessionId = await this.CreateSessionAsync();

        var createRequest = new CreateSessionTagRequest
        {
            SessionId = sessionId,
            Tag = "test-tag",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-tags/", createRequest);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<SessionTagResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-tags/{createdTag!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateMultipleTags_ForSameSession_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionId = await this.CreateSessionAsync();

        var tag1Request = new CreateSessionTagRequest
        {
            SessionId = sessionId,
            Tag = "birthday",
        };

        var tag2Request = new CreateSessionTagRequest
        {
            SessionId = sessionId,
            Tag = "celebration",
        };

        // Act
        var response1 = await this.client.PostAsJsonAsync("/api/session-tags/", tag1Request);
        var response2 = await this.client.PostAsJsonAsync("/api/session-tags/", tag2Request);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify both tags exist for the session
        var getResponse = await this.client.GetAsync($"/api/session-tags/session/{sessionId}");
        var tags = await getResponse.Content.ReadFromJsonAsync<List<SessionTagResponse>>();
        tags.Should().NotBeNull();
        tags!.Count.Should().Be(2);
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
