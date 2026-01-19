// <copyright file="AchievementEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Achievements;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class AchievementEndpoints
{
    public static void MapAchievementEndpoints(this IEndpointRouteBuilder app)
    {
        var achievementApi = app.MapGroup("/api/achievements")
            .WithTags("Achievements")
            .RequireAuthorization();

        achievementApi.MapGet("/", GetAllAchievements)
            .WithName("GetAllAchievements")
            .WithOpenApi();

        achievementApi.MapGet("/{id:guid}", GetAchievementById)
            .WithName("GetAchievementById")
            .WithOpenApi();

        achievementApi.MapGet("/type/{type}", GetAchievementsByType)
            .WithName("GetAchievementsByType")
            .WithOpenApi();

        achievementApi.MapPost("/", CreateAchievement)
            .WithName("CreateAchievement")
            .WithOpenApi();

        achievementApi.MapPut("/{id:guid}", UpdateAchievement)
            .WithName("UpdateAchievement")
            .WithOpenApi();

        achievementApi.MapDelete("/{id:guid}", DeleteAchievement)
            .WithName("DeleteAchievement")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllAchievements(
        IAchievementRepository achievementRepository)
    {
        var achievements = await achievementRepository.GetAllAsync();
        var response = achievements.Select(a => MapToResponse(a));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetAchievementById(
        Guid id,
        IAchievementRepository achievementRepository)
    {
        var achievement = await achievementRepository.GetByIdAsync(id);

        if (achievement == null)
        {
            return Results.NotFound(new { message = "Achievement not found" });
        }

        return Results.Ok(MapToResponse(achievement));
    }

    private static async Task<IResult> GetAchievementsByType(
        string type,
        IAchievementRepository achievementRepository)
    {
        var achievements = await achievementRepository.GetByTypeAsync(type);
        var response = achievements.Select(a => MapToResponse(a));
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateAchievement(
        CreateAchievementRequest request,
        ClaimsPrincipal user,
        IAchievementRepository achievementRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Icon = request.Icon,
            Criteria = request.Criteria,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await achievementRepository.CreateAsync(achievement);
        return Results.Created($"/api/achievements/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateAchievement(
        Guid id,
        UpdateAchievementRequest request,
        ClaimsPrincipal user,
        IAchievementRepository achievementRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var achievement = await achievementRepository.GetByIdAsync(id);
        if (achievement == null)
        {
            return Results.NotFound(new { message = "Achievement not found" });
        }

        if (achievement.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        if (request.Name != null)
        {
            achievement.Name = request.Name;
        }

        if (request.Description != null)
        {
            achievement.Description = request.Description;
        }

        if (request.Type != null)
        {
            achievement.Type = request.Type;
        }

        if (request.Icon != null)
        {
            achievement.Icon = request.Icon;
        }

        if (request.Criteria != null)
        {
            achievement.Criteria = request.Criteria;
        }

        achievement.UpdatedById = currentUser.Id;
        achievement.UpdatedAt = DateTime.UtcNow;

        var updated = await achievementRepository.UpdateAsync(achievement);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteAchievement(
        Guid id,
        ClaimsPrincipal user,
        IAchievementRepository achievementRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var achievement = await achievementRepository.GetByIdAsync(id);
        if (achievement == null)
        {
            return Results.NotFound(new { message = "Achievement not found" });
        }

        if (achievement.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await achievementRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete achievement");
        }

        return Results.NoContent();
    }

    private static AchievementResponse MapToResponse(Achievement achievement)
    {
        return new AchievementResponse
        {
            Id = achievement.Id,
            Name = achievement.Name,
            Description = achievement.Description,
            Type = achievement.Type,
            Icon = achievement.Icon,
            Criteria = achievement.Criteria,
            CreatedAt = achievement.CreatedAt,
        };
    }
}
