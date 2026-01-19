// <copyright file="DrinkEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Drinks;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class DrinkEndpoints
{
    public static void MapDrinkEndpoints(this IEndpointRouteBuilder app)
    {
        var drinkApi = app.MapGroup("/api/drinks")
            .WithTags("Drinks")
            .RequireAuthorization();

        drinkApi.MapGet("/", GetAllDrinks)
            .WithName("GetAllDrinks")
            .WithOpenApi();

        drinkApi.MapGet("/{id:guid}", GetDrinkById)
            .WithName("GetDrinkById")
            .WithOpenApi();

        drinkApi.MapGet("/type/{drinkTypeId:guid}", GetDrinksByType)
            .WithName("GetDrinksByType")
            .WithOpenApi();

        drinkApi.MapGet("/search", SearchDrinks)
            .WithName("SearchDrinks")
            .WithOpenApi();

        drinkApi.MapPost("/", CreateDrink)
            .WithName("CreateDrink")
            .WithOpenApi();

        drinkApi.MapPut("/{id:guid}", UpdateDrink)
            .WithName("UpdateDrink")
            .WithOpenApi();

        drinkApi.MapDelete("/{id:guid}", DeleteDrink)
            .WithName("DeleteDrink")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllDrinks(
        IDrinkRepository drinkRepository)
    {
        var drinks = await drinkRepository.GetAllAsync();
        var response = drinks.Select(d => MapToResponse(d));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetDrinkById(
        Guid id,
        IDrinkRepository drinkRepository)
    {
        var drink = await drinkRepository.GetByIdAsync(id);

        if (drink == null)
        {
            return Results.NotFound(new { message = "Drink not found" });
        }

        return Results.Ok(MapToResponse(drink));
    }

    private static async Task<IResult> GetDrinksByType(
        Guid drinkTypeId,
        IDrinkRepository drinkRepository)
    {
        var drinks = await drinkRepository.GetByDrinkTypeIdAsync(drinkTypeId);
        var response = drinks.Select(d => MapToResponse(d));
        return Results.Ok(response);
    }

    private static async Task<IResult> SearchDrinks(
        string name,
        IDrinkRepository drinkRepository)
    {
        var drinks = await drinkRepository.SearchByNameAsync(name);
        var response = drinks.Select(d => MapToResponse(d));
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateDrink(
        CreateDrinkRequest request,
        ClaimsPrincipal user,
        IDrinkRepository drinkRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var drink = new Drink
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            DrinkTypeId = request.DrinkTypeId,
            AlcoholContent = request.AlcoholContent,
            VolumeLitres = request.VolumeLitres,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await drinkRepository.CreateAsync(drink);
        return Results.Created($"/api/drinks/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateDrink(
        Guid id,
        UpdateDrinkRequest request,
        ClaimsPrincipal user,
        IDrinkRepository drinkRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var drink = await drinkRepository.GetByIdAsync(id);
        if (drink == null)
        {
            return Results.NotFound(new { message = "Drink not found" });
        }

        if (drink.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        if (request.Name != null)
        {
            drink.Name = request.Name;
        }

        if (request.Description != null)
        {
            drink.Description = request.Description;
        }

        if (request.DrinkTypeId.HasValue)
        {
            drink.DrinkTypeId = request.DrinkTypeId.Value;
        }

        if (request.AlcoholContent.HasValue)
        {
            drink.AlcoholContent = request.AlcoholContent.Value;
        }

        if (request.VolumeLitres.HasValue)
        {
            drink.VolumeLitres = request.VolumeLitres.Value;
        }

        drink.UpdatedById = currentUser.Id;
        drink.UpdatedAt = DateTime.UtcNow;

        var updated = await drinkRepository.UpdateAsync(drink);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteDrink(
        Guid id,
        ClaimsPrincipal user,
        IDrinkRepository drinkRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var drink = await drinkRepository.GetByIdAsync(id);
        if (drink == null)
        {
            return Results.NotFound(new { message = "Drink not found" });
        }

        if (drink.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await drinkRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete drink");
        }

        return Results.NoContent();
    }

    private static DrinkResponse MapToResponse(Drink drink)
    {
        return new DrinkResponse
        {
            Id = drink.Id,
            Name = drink.Name,
            Description = drink.Description,
            DrinkTypeId = drink.DrinkTypeId,
            AlcoholContent = drink.AlcoholContent,
            VolumeLitres = drink.VolumeLitres,
            CreatedById = drink.CreatedById,
            CreatedAt = drink.CreatedAt,
            UpdatedAt = drink.UpdatedAt,
        };
    }
}
