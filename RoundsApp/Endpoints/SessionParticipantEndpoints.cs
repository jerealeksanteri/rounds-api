// <copyright file="SessionParticipantEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.DTOs.Users;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class SessionParticipantEndpoints
{
    public static void MapSessionParticipantEndpoints(this IEndpointRouteBuilder app)
    {
        var participantApi = app.MapGroup("/api/session-participants")
            .WithTags("Session Participants")
            .RequireAuthorization();

        participantApi.MapGet("/session/{sessionId:guid}", GetParticipantsBySession)
            .WithName("GetParticipantsBySession")
            .WithOpenApi();

        participantApi.MapGet("/user/{userId:guid}", GetParticipantsByUser)
            .WithName("GetParticipantsByUser")
            .WithOpenApi();

        participantApi.MapPost("/", AddParticipant)
            .WithName("AddParticipant")
            .WithOpenApi();

        participantApi.MapDelete("/{id:guid}", RemoveParticipant)
            .WithName("RemoveParticipant")
            .WithOpenApi();
    }

    private static async Task<IResult> GetParticipantsBySession(
        Guid sessionId,
        ISessionParticipantRepository participantRepository)
    {
        var participants = await participantRepository.GetBySessionIdAsync(sessionId);
        var response = participants.Select(p => MapToResponse(p));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetParticipantsByUser(
        Guid userId,
        ISessionParticipantRepository participantRepository)
    {
        var participants = await participantRepository.GetByUserIdAsync(userId);
        var response = participants.Select(p => MapToResponse(p));
        return Results.Ok(response);
    }

    private static async Task<IResult> AddParticipant(
        CreateParticipantRequest request,
        ClaimsPrincipal user,
        ISessionParticipantRepository participantRepository,
        IDrinkingSessionRepository sessionRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var session = await sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null)
        {
            return Results.NotFound(new { message = "Session not found" });
        }

        var participant = new SessionParticipant
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            UserId = request.UserId,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await participantRepository.CreateAsync(participant);
        return Results.Created($"/api/session-participants/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> RemoveParticipant(
        Guid id,
        ClaimsPrincipal user,
        ISessionParticipantRepository participantRepository,
        IDrinkingSessionRepository sessionRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var participant = await participantRepository.GetByIdAsync(id);
        if (participant == null)
        {
            return Results.NotFound(new { message = "Participant not found" });
        }

        var session = await sessionRepository.GetByIdAsync(participant.SessionId);
        if (session == null)
        {
            return Results.NotFound(new { message = "Session not found" });
        }

        if (session.CreatedById != currentUser.Id && participant.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await participantRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to remove participant");
        }

        return Results.NoContent();
    }

    private static ParticipantResponse MapToResponse(SessionParticipant participant)
    {
        return new ParticipantResponse
        {
            Id = participant.Id,
            SessionId = participant.SessionId,
            UserId = participant.UserId,
            User = participant.User != null ? MapUserToResponse(participant.User) : null,
            CreatedAt = participant.CreatedAt,
            CreatedById = participant.CreatedById,
        };
    }

    private static UserResponse MapUserToResponse(ApplicationUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
        };
    }
}
