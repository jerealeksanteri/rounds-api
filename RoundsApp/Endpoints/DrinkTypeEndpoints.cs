// <copyright file="DrinkTypeEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Drinks;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class DrinkTypeEndpoints
{
    public static void MapDrinkTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var drinkTypeApi = app.MapGroup("/api/drink-types")
            .WithTags("Drink Types")
            .RequireAuthorization();

        drinkTypeApi.MapGet("/", GetAllDrinkTypes)
            .WithName("GetAllDrinkTypes")
            .WithOpenApi();

        drinkTypeApi.MapGet("/{id:guid}", GetDrinkTypeById)
            .WithName("GetDrinkTypeById")
            .WithOpenApi();

        drinkTypeApi.MapPost("/", CreateDrinkType)
            .WithName("CreateDrinkType")
            .WithOpenApi();

        drinkTypeApi.MapPut("/{id:guid}", UpdateDrinkType)
            .WithName("UpdateDrinkType")
            .WithOpenApi();

        drinkTypeApi.MapDelete("/{id:guid}", DeleteDrinkType)
            .WithName("DeleteDrinkType")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllDrinkTypes(
        IDrinkTypeRepository drinkTypeRepository)
    {
        var drinkTypes = await drinkTypeRepository.GetAllAsync();
        var response = drinkTypes.Select(dt => MapToResponse(dt));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetDrinkTypeById(
        Guid id,
        IDrinkTypeRepository drinkTypeRepository)
    {
        var drinkType = await drinkTypeRepository.GetByIdAsync(id);

        if (drinkType == null)
        {
            return Results.NotFound(new { message = "Drink type not found" });
        }

        return Results.Ok(MapToResponse(drinkType));
    }

    private static async Task<IResult> CreateDrinkType(
        CreateDrinkTypeRequest request,
        ClaimsPrincipal user,
        IDrinkTypeRepository drinkTypeRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var drinkType = new DrinkType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await drinkTypeRepository.CreateAsync(drinkType);
        return Results.Created($"/api/drink-types/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateDrinkType(
        Guid id,
        UpdateDrinkTypeRequest request,
        ClaimsPrincipal user,
        IDrinkTypeRepository drinkTypeRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var drinkType = await drinkTypeRepository.GetByIdAsync(id);
        if (drinkType == null)
        {
            return Results.NotFound(new { message = "Drink type not found" });
        }

        if (drinkType.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        if (request.Name != null)
        {
            drinkType.Name = request.Name;
        }

        if (request.Description != null)
        {
            drinkType.Description = request.Description;
        }

        drinkType.UpdatedById = currentUser.Id;
        drinkType.UpdatedAt = DateTime.UtcNow;

        var updated = await drinkTypeRepository.UpdateAsync(drinkType);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteDrinkType(
        Guid id,
        ClaimsPrincipal user,
        IDrinkTypeRepository drinkTypeRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var drinkType = await drinkTypeRepository.GetByIdAsync(id);
        if (drinkType == null)
        {
            return Results.NotFound(new { message = "Drink type not found" });
        }

        if (drinkType.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await drinkTypeRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete drink type");
        }

        return Results.NoContent();
    }

    private static DrinkTypeResponse MapToResponse(DrinkType drinkType)
    {
        return new DrinkTypeResponse
        {
            Id = drinkType.Id,
            Name = drinkType.Name,
            Description = drinkType.Description,
            CreatedAt = drinkType.CreatedAt,
            CreatedById = drinkType.CreatedById,
        };
    }
}
