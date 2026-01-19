// <copyright file="AchievementEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.Achievements;
using Xunit;

namespace RoundsApp.Tests;

public class AchievementEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public AchievementEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetAllAchievements_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/achievements/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllAchievements_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/achievements/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();
        achievements.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAchievement_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateAchievementRequest
        {
            Name = "First Drink",
            Description = "Logged your first drink",
            Type = "milestone",
            Icon = "🍺",
            Criteria = "{\"drinks_count\": 1}",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/achievements/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AchievementResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("First Drink");
        result.Type.Should().Be("milestone");
    }

    [Fact]
    public async Task CreateAchievement_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateAchievementRequest
        {
            Name = "Test Achievement",
            Description = "Test",
            Type = "milestone",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/achievements/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAchievementById_WithValidId_ReturnsAchievement()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateAchievementRequest
        {
            Name = "Test Achievement",
            Description = "Test Description",
            Type = "badge",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/achievements/", createRequest);
        var createdAchievement = await createResponse.Content.ReadFromJsonAsync<AchievementResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/achievements/{createdAchievement!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AchievementResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdAchievement.Id);
        result.Name.Should().Be("Test Achievement");
    }

    [Fact]
    public async Task GetAchievementById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/achievements/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAchievementsByType_ReturnsFilteredAchievements()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateAchievementRequest
        {
            Name = "Test Achievement",
            Description = "Test Description",
            Type = "badge",
        };

        await this.client.PostAsJsonAsync("/api/achievements/", createRequest);

        // Act
        var response = await this.client.GetAsync("/api/achievements/type/badge");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();
        achievements.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAchievement_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateAchievementRequest
        {
            Name = "Original Name",
            Description = "Original Description",
            Type = "milestone",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/achievements/", createRequest);
        var createdAchievement = await createResponse.Content.ReadFromJsonAsync<AchievementResponse>();

        var updateRequest = new UpdateAchievementRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/achievements/{createdAchievement!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AchievementResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateAchievement_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateAchievementRequest
        {
            Name = "Original Name",
            Description = "Original Description",
            Type = "milestone",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/achievements/", createRequest);
        var createdAchievement = await createResponse.Content.ReadFromJsonAsync<AchievementResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateAchievementRequest
        {
            Name = "Updated Name",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/achievements/{createdAchievement!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAchievement_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateAchievementRequest
        {
            Name = "Test Achievement",
            Description = "Test Description",
            Type = "badge",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/achievements/", createRequest);
        var createdAchievement = await createResponse.Content.ReadFromJsonAsync<AchievementResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/achievements/{createdAchievement!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await this.client.GetAsync($"/api/achievements/{createdAchievement.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAchievement_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateAchievementRequest
        {
            Name = "Test Achievement",
            Description = "Test Description",
            Type = "badge",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/achievements/", createRequest);
        var createdAchievement = await createResponse.Content.ReadFromJsonAsync<AchievementResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/achievements/{createdAchievement!.Id}");

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
}
