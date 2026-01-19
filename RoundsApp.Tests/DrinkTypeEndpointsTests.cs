// <copyright file="DrinkTypeEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.Drinks;
using Xunit;

namespace RoundsApp.Tests;

public class DrinkTypeEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private readonly WebApplicationFactory<Program> factory;

    public DrinkTypeEndpointsTests(WebApplicationFactory<Program> factory)
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

        this.factory = customFactory;
        this.client = customFactory.CreateClient();
    }

    [Fact]
    public async Task GetAllDrinkTypes_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/drink-types/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllDrinkTypes_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/drink-types/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var drinkTypes = await response.Content.ReadFromJsonAsync<List<DrinkTypeResponse>>();
        drinkTypes.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDrinkType_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateDrinkTypeRequest
        {
            Name = "Beer",
            Description = "Alcoholic beverage made from grains",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<DrinkTypeResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Beer");
        result.Description.Should().Be("Alcoholic beverage made from grains");
    }

    [Fact]
    public async Task CreateDrinkType_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateDrinkTypeRequest
        {
            Name = "Beer",
            Description = "Test Description",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDrinkTypeById_WithValidId_ReturnsDrinkType()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateDrinkTypeRequest
        {
            Name = "Wine",
            Description = "Alcoholic beverage made from grapes",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);
        var createdDrinkType = await createResponse.Content.ReadFromJsonAsync<DrinkTypeResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/drink-types/{createdDrinkType!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DrinkTypeResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdDrinkType.Id);
        result.Name.Should().Be("Wine");
    }

    [Fact]
    public async Task GetDrinkTypeById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/drink-types/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDrinkType_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateDrinkTypeRequest
        {
            Name = "Original Name",
            Description = "Original Description",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);
        var createdDrinkType = await createResponse.Content.ReadFromJsonAsync<DrinkTypeResponse>();

        var updateRequest = new UpdateDrinkTypeRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/drink-types/{createdDrinkType!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DrinkTypeResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateDrinkType_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateDrinkTypeRequest
        {
            Name = "Original Name",
            Description = "Original Description",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);
        var createdDrinkType = await createResponse.Content.ReadFromJsonAsync<DrinkTypeResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateDrinkTypeRequest
        {
            Name = "Updated Name",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/drink-types/{createdDrinkType!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteDrinkType_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateDrinkTypeRequest
        {
            Name = "Spirits",
            Description = "Distilled alcoholic beverages",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);
        var createdDrinkType = await createResponse.Content.ReadFromJsonAsync<DrinkTypeResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/drink-types/{createdDrinkType!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await this.client.GetAsync($"/api/drink-types/{createdDrinkType.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDrinkType_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createRequest = new CreateDrinkTypeRequest
        {
            Name = "Cocktails",
            Description = "Mixed drinks",
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);
        var createdDrinkType = await createResponse.Content.ReadFromJsonAsync<DrinkTypeResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/drink-types/{createdDrinkType!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateDrinkType_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new CreateDrinkTypeRequest
        {
            Name = string.Empty,
            Description = "Test Description",
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/drink-types/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
