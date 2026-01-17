// <copyright file="DrinkingSessionIntegrationTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.Models;
using Xunit;

namespace RoundsApp.Tests;

public class DrinkingSessionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private string? authToken;

    public DrinkingSessionIntegrationTests(WebApplicationFactory<Program> factory)
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

    private async Task<string> GetAuthTokenAsync()
    {
        if (this.authToken != null)
        {
            return this.authToken;
        }

        // Register a new user
        var registerRequest = new RegisterRequest
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "Test123!@#",
            FirstName = "Test",
            LastName = "User",
            UserName = $"testuser{Guid.NewGuid().ToString()[..8]}",
        };

        await this.client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Login to get token
        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password,
        };

        var response = await this.client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        this.authToken = authResponse!.Token;
        return this.authToken;
    }

    private async Task SetAuthorizationHeader()
    {
        var token = await this.GetAuthTokenAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CreateDrinkingSession_WithAuthentication_ReturnsCreatedSession()
    {
        // Arrange
        await this.SetAuthorizationHeader();
        var request = new CreateDrinkingSessionRequest
        {
            Name = "Test Session",
            Description = "Integration test session",
            ScheduledAt = DateTime.UtcNow.AddDays(1),
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/drinkingsessions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<DrinkingSession>();
        session.Should().NotBeNull();
        session!.Title.Should().Be(request.Name);
        session.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task CreateDrinkingSession_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        this.client.DefaultRequestHeaders.Authorization = null;
        var request = new CreateDrinkingSessionRequest
        {
            Name = "Test Session",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/drinkingsessions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateDrinkingSession_AsCreator_ReturnsUpdatedSession()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session first
        var createRequest = new CreateDrinkingSessionRequest { Name = "Original" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        // Update the session
        var updateRequest = new CreateDrinkingSessionRequest
        {
            Name = "Updated Session",
            Description = "Updated description",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/drinkingsessions/{createdSession!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedSession = await response.Content.ReadFromJsonAsync<DrinkingSession>();
        updatedSession!.Title.Should().Be(updateRequest.Name);
        updatedSession.Description.Should().Be(updateRequest.Description);
    }

    [Fact]
    public async Task UpdateDrinkingSession_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await this.SetAuthorizationHeader();
        var request = new CreateDrinkingSessionRequest { Name = "Updated" };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/drinkingsessions/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDrinkingSession_AsCreator_ReturnsNoContent()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session first
        var createRequest = new CreateDrinkingSessionRequest { Name = "To Delete" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        // Act
        var response = await this.client.DeleteAsync($"/api/drinkingsessions/{createdSession!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDrinkingSession_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Act
        var response = await this.client.DeleteAsync($"/api/drinkingsessions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddParticipant_ToValidSession_ReturnsOk()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session
        var createRequest = new CreateDrinkingSessionRequest { Name = "Session with participants" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var session = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        // Act - Add participant (the authenticated user joins the session)
        var response = await this.client.PostAsync($"/api/drinkingsessions/{session!.Id}/participants", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetParticipants_ReturnsParticipantList()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session and add participant
        var createRequest = new CreateDrinkingSessionRequest { Name = "Session" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var session = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        await this.client.PostAsync($"/api/drinkingsessions/{session!.Id}/participants", null);

        // Act
        var response = await this.client.GetAsync($"/api/drinkingsessions/{session.Id}/participants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var participants = await response.Content.ReadFromJsonAsync<List<DrinkingSessionParticipation>>();
        participants.Should().NotBeNull();
        participants!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddImage_ToValidSession_ReturnsOk()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session
        var createRequest = new CreateDrinkingSessionRequest { Name = "Session with images" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var session = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        var imageRequest = new AddImageRequest
        {
            ImageData = new byte[] { 1, 2, 3, 4, 5 },
        };

        // Act
        var response = await this.client.PostAsJsonAsync($"/api/drinkingsessions/{session!.Id}/images", imageRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetImages_ReturnsImageList()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session and add image
        var createRequest = new CreateDrinkingSessionRequest { Name = "Session" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var session = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        var imageRequest = new AddImageRequest { ImageData = new byte[] { 1, 2, 3 } };
        await this.client.PostAsJsonAsync($"/api/drinkingsessions/{session!.Id}/images", imageRequest);

        // Act
        var response = await this.client.GetAsync($"/api/drinkingsessions/{session.Id}/images");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var images = await response.Content.ReadFromJsonAsync<List<DrinkingSessionImage>>();
        images.Should().NotBeNull();
        images!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTotalDrinkCount_ReturnsCount()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session
        var createRequest = new CreateDrinkingSessionRequest { Name = "Session" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var session = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        // Act
        var response = await this.client.GetAsync($"/api/drinkingsessions/{session!.Id}/totaldrinkcount");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var count = await response.Content.ReadFromJsonAsync<int>();
        count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetDrinkCountsPerParticipant_ReturnsDictionary()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create a session
        var createRequest = new CreateDrinkingSessionRequest { Name = "Session" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var session = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        // Act
        var response = await this.client.GetAsync($"/api/drinkingsessions/{session!.Id}/drinkcountsperparticipant");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var counts = await response.Content.ReadFromJsonAsync<Dictionary<Guid, int>>();
        counts.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateDrinkingSession_AsNonCreator_ReturnsForbidden()
    {
        // Arrange
        await this.SetAuthorizationHeader();

        // Create session with first user
        var createRequest = new CreateDrinkingSessionRequest { Name = "Session" };
        var createResponse = await this.client.PostAsJsonAsync("/api/drinkingsessions", createRequest);
        var session = await createResponse.Content.ReadFromJsonAsync<DrinkingSession>();

        // Create and login as second user
        var secondUserRegister = new RegisterRequest
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "Test123!@#",
            FirstName = "Second",
            LastName = "User",
            UserName = $"testuser{Guid.NewGuid().ToString()[..8]}",
        };

        await this.client.PostAsJsonAsync("/api/auth/register", secondUserRegister);

        var loginRequest = new LoginRequest
        {
            Email = secondUserRegister.Email,
            Password = secondUserRegister.Password,
        };

        var loginResponse = await this.client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Set second user's token
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Try to update first user's session
        var updateRequest = new CreateDrinkingSessionRequest { Name = "Hacked" };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/drinkingsessions/{session!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
