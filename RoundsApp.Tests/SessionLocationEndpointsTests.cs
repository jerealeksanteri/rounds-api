// <copyright file="SessionLocationEndpointsTests.cs" company="RoundsApp">
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

public class SessionLocationEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SessionLocationEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetAllLocations_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/session-locations/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllLocations_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/session-locations/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var locations = await response.Content.ReadFromJsonAsync<List<SessionLocationResponse>>();
        locations.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateLocation_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Test Bar",
            Address = "123 Test Street",
            Latitude = (decimal?)60.1699,
            Longitude = (decimal?)24.9384,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SessionLocationResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Bar");
        result.Address.Should().Be("123 Test Street");
        result.Latitude.Should().Be(60.1699m);
        result.Longitude.Should().Be(24.9384m);
    }

    [Fact]
    public async Task CreateLocation_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Test Bar",
            Address = "123 Test Street",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLocationById_WithValidId_ReturnsLocation()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Test Bar",
            Address = "123 Test Street",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);
        var createdLocation = await createResponse.Content.ReadFromJsonAsync<SessionLocationResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/session-locations/{createdLocation!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionLocationResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdLocation.Id);
        result.Name.Should().Be("Test Bar");
    }

    [Fact]
    public async Task GetLocationById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/session-locations/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchLocations_WithMatchingName_ReturnsLocations()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Central Bar and Grill",
            Address = "123 Test Street",
        };

        await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);

        // Act
        var response = await this.client.GetAsync("/api/session-locations/search?name=Central");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var locations = await response.Content.ReadFromJsonAsync<List<SessionLocationResponse>>();
        locations.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLocation_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Original Name",
            Address = "Original Address",
            Latitude = (decimal?)60.1699,
            Longitude = (decimal?)24.9384,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);
        var createdLocation = await createResponse.Content.ReadFromJsonAsync<SessionLocationResponse>();

        var updateRequest = new UpdateSessionLocationRequest
        {
            Name = "Updated Name",
            Address = "Updated Address",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-locations/{createdLocation!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionLocationResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Address.Should().Be("Updated Address");
    }

    [Fact]
    public async Task UpdateLocation_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Original Name",
            Address = "Original Address",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);
        var createdLocation = await createResponse.Content.ReadFromJsonAsync<SessionLocationResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateSessionLocationRequest
        {
            Name = "Updated Name",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/session-locations/{createdLocation!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLocation_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Test Bar",
            Address = "123 Test Street",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);
        var createdLocation = await createResponse.Content.ReadFromJsonAsync<SessionLocationResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/session-locations/{createdLocation!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await this.client.GetAsync($"/api/session-locations/{createdLocation.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteLocation_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateSessionLocationRequest
        {
            Name = "Test Bar",
            Address = "123 Test Street",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/session-locations/", createRequest);
        var createdLocation = await createResponse.Content.ReadFromJsonAsync<SessionLocationResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/session-locations/{createdLocation!.Id}");

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
