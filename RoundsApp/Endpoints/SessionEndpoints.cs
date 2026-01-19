// <copyright file="SessionEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var sessionApi = app.MapGroup("/api/sessions")
            .WithTags("Sessions")
            .RequireAuthorization();

        sessionApi.MapGet("/", GetAllSessions)
            .WithName("GetAllSessions")
            .WithOpenApi();

        sessionApi.MapGet("/{id:guid}", GetSessionById)
            .WithName("GetSessionById")
            .WithOpenApi();

        sessionApi.MapGet("/user/{userId:guid}", GetSessionsByUserId)
            .WithName("GetSessionsByUserId")
            .WithOpenApi();

        sessionApi.MapGet("/upcoming", GetUpcomingSessions)
            .WithName("GetUpcomingSessions")
            .WithOpenApi();

        sessionApi.MapPost("/", CreateSession)
            .WithName("CreateSession")
            .WithOpenApi();

        sessionApi.MapPut("/{id:guid}", UpdateSession)
            .WithName("UpdateSession")
            .WithOpenApi();

        sessionApi.MapDelete("/{id:guid}", DeleteSession)
            .WithName("DeleteSession")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllSessions(
        IDrinkingSessionRepository sessionRepository)
    {
        var sessions = await sessionRepository.GetAllAsync();
        var response = sessions.Select(s => MapToResponse(s));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetSessionById(
        Guid id,
        IDrinkingSessionRepository sessionRepository)
    {
        var session = await sessionRepository.GetByIdAsync(id);

        if (session == null)
        {
            return Results.NotFound(new { message = "Session not found" });
        }

        return Results.Ok(MapToResponse(session));
    }

    private static async Task<IResult> GetSessionsByUserId(
        Guid userId,
        IDrinkingSessionRepository sessionRepository)
    {
        var sessions = await sessionRepository.GetByUserIdAsync(userId);
        var response = sessions.Select(s => MapToResponse(s));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetUpcomingSessions(
        IDrinkingSessionRepository sessionRepository)
    {
        var sessions = await sessionRepository.GetUpcomingSessionsAsync();
        var response = sessions.Select(s => MapToResponse(s));
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateSession(
        CreateSessionRequest request,
        ClaimsPrincipal user,
        IDrinkingSessionRepository sessionRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var session = new DrinkingSession
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            LocationId = request.LocationId,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await sessionRepository.CreateAsync(session);
        return Results.Created($"/api/sessions/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateSession(
        Guid id,
        UpdateSessionRequest request,
        ClaimsPrincipal user,
        IDrinkingSessionRepository sessionRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var session = await sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return Results.NotFound(new { message = "Session not found" });
        }

        if (session.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        if (request.Name != null)
        {
            session.Name = request.Name;
        }

        if (request.Description != null)
        {
            session.Description = request.Description;
        }

        if (request.StartsAt.HasValue)
        {
            session.StartsAt = request.StartsAt;
        }

        if (request.EndsAt.HasValue)
        {
            session.EndsAt = request.EndsAt;
        }

        if (request.LocationId.HasValue)
        {
            session.LocationId = request.LocationId;
        }

        session.UpdatedById = currentUser.Id;
        session.UpdatedAt = DateTime.UtcNow;

        var updated = await sessionRepository.UpdateAsync(session);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteSession(
        Guid id,
        ClaimsPrincipal user,
        IDrinkingSessionRepository sessionRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var session = await sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return Results.NotFound(new { message = "Session not found" });
        }

        if (session.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await sessionRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete session");
        }

        return Results.NoContent();
    }

    private static SessionResponse MapToResponse(DrinkingSession session)
    {
        return new SessionResponse
        {
            Id = session.Id,
            Name = session.Name,
            Description = session.Description,
            StartsAt = session.StartsAt,
            EndsAt = session.EndsAt,
            LocationId = session.LocationId,
            CreatedById = session.CreatedById,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
        };
    }
}
