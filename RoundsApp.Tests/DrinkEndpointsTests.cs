// <copyright file="DrinkEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RoundsApp.DTOs;
using RoundsApp.DTOs.Drinks;
using Xunit;

namespace RoundsApp.Tests;

public class DrinkEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public DrinkEndpointsTests(WebApplicationFactory<Program> factory)
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
    public async Task GetAllDrinks_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await this.client.GetAsync("/api/drinks/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllDrinks_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await this.client.GetAsync("/api/drinks/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var drinks = await response.Content.ReadFromJsonAsync<List<DrinkResponse>>();
        drinks.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDrink_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Test Beer",
            Description = "Test Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/drinks/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Beer");
        result.AlcoholContent.Should().Be(5.0m);
        result.VolumeLitres.Should().Be(0.5m);
    }

    [Fact]
    public async Task CreateDrink_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateDrinkRequest
        {
            Name = "Test Beer",
            Description = "Test Description",
            DrinkTypeId = Guid.NewGuid(),
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/api/drinks/", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDrinkById_WithValidId_ReturnsDrink()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Test Beer",
            Description = "Test Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drinks/", createRequest);
        var createdDrink = await createResponse.Content.ReadFromJsonAsync<DrinkResponse>();

        // Act
        var response = await this.client.GetAsync($"/api/drinks/{createdDrink!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdDrink.Id);
        result.Name.Should().Be("Test Beer");
    }

    [Fact]
    public async Task GetDrinkById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await this.client.GetAsync($"/api/drinks/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDrink_WithValidData_ReturnsOk()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Original Name",
            Description = "Original Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drinks/", createRequest);
        var createdDrink = await createResponse.Content.ReadFromJsonAsync<DrinkResponse>();

        var updateRequest = new UpdateDrinkRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            AlcoholContent = 6.0m,
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/drinks/{createdDrink!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DrinkResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.AlcoholContent.Should().Be(6.0m);
    }

    [Fact]
    public async Task UpdateDrink_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Original Name",
            Description = "Original Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drinks/", createRequest);
        var createdDrink = await createResponse.Content.ReadFromJsonAsync<DrinkResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updateRequest = new UpdateDrinkRequest
        {
            Name = "Updated Name",
        };

        // Act
        var response = await this.client.PutAsJsonAsync($"/api/drinks/{createdDrink!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteDrink_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Test Beer",
            Description = "Test Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drinks/", createRequest);
        var createdDrink = await createResponse.Content.ReadFromJsonAsync<DrinkResponse>();

        // Act
        var response = await this.client.DeleteAsync($"/api/drinks/{createdDrink!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await this.client.GetAsync($"/api/drinks/{createdDrink.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDrink_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var token1 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Test Beer",
            Description = "Test Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        var createResponse = await this.client.PostAsJsonAsync("/api/drinks/", createRequest);
        var createdDrink = await createResponse.Content.ReadFromJsonAsync<DrinkResponse>();

        // Login as different user
        var token2 = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await this.client.DeleteAsync($"/api/drinks/{createdDrink!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SearchDrinks_WithMatchingName_ReturnsDrinks()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Special IPA Beer",
            Description = "Test Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        await this.client.PostAsJsonAsync("/api/drinks/", createRequest);

        // Act
        var response = await this.client.GetAsync("/api/drinks/search?name=IPA");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var drinks = await response.Content.ReadFromJsonAsync<List<DrinkResponse>>();
        drinks.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDrinksByType_WithValidTypeId_ReturnsDrinks()
    {
        // Arrange
        var token = await this.RegisterAndLoginAsync();
        this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var drinkTypeId = await this.CreateDrinkTypeAsync();

        var createRequest = new CreateDrinkRequest
        {
            Name = "Test Beer",
            Description = "Test Description",
            DrinkTypeId = drinkTypeId,
            AlcoholContent = 5.0m,
            VolumeLitres = 0.5m,
        };

        await this.client.PostAsJsonAsync("/api/drinks/", createRequest);

        // Act
        var response = await this.client.GetAsync($"/api/drinks/type/{drinkTypeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var drinks = await response.Content.ReadFromJsonAsync<List<DrinkResponse>>();
        drinks.Should().NotBeNull();
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

    private async Task<Guid> CreateDrinkTypeAsync()
    {
        var createTypeRequest = new CreateDrinkTypeRequest
        {
            Name = $"Test Type {Guid.NewGuid().ToString()[..8]}",
            Description = "Test Type Description",
        };

        var response = await this.client.PostAsJsonAsync("/api/drink-types/", createTypeRequest);
        var drinkType = await response.Content.ReadFromJsonAsync<DrinkTypeResponse>();
        return drinkType!.Id;
    }
}
