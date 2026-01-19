// <copyright file="SessionInviteEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.DTOs.Users;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class SessionInviteEndpoints
{
    public static void MapSessionInviteEndpoints(this IEndpointRouteBuilder app)
    {
        var inviteApi = app.MapGroup("/api/session-invites")
            .WithTags("Session Invites")
            .RequireAuthorization();

        inviteApi.MapGet("/session/{sessionId:guid}", GetInvitesBySession)
            .WithName("GetInvitesBySession")
            .WithOpenApi();

        inviteApi.MapGet("/my-invites", GetMyInvites)
            .WithName("GetMyInvites")
            .WithOpenApi();

        inviteApi.MapGet("/pending", GetPendingInvites)
            .WithName("GetPendingInvites")
            .WithOpenApi();

        inviteApi.MapGet("/{id:guid}", GetInviteById)
            .WithName("GetInviteById")
            .WithOpenApi();

        inviteApi.MapPost("/", CreateInvite)
            .WithName("CreateInvite")
            .WithOpenApi();

        inviteApi.MapPut("/{id:guid}", UpdateInvite)
            .WithName("UpdateInvite")
            .WithOpenApi();

        inviteApi.MapDelete("/{id:guid}", DeleteInvite)
            .WithName("DeleteInvite")
            .WithOpenApi();
    }

    private static async Task<IResult> GetInvitesBySession(
        Guid sessionId,
        ISessionInviteRepository inviteRepository)
    {
        var invites = await inviteRepository.GetBySessionIdAsync(sessionId);
        var response = invites.Select(i => MapToResponse(i));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetMyInvites(
        ClaimsPrincipal user,
        ISessionInviteRepository inviteRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var invites = await inviteRepository.GetByUserIdAsync(currentUser.Id);
        var response = invites.Select(i => MapToResponse(i));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetPendingInvites(
        ClaimsPrincipal user,
        ISessionInviteRepository inviteRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var invites = await inviteRepository.GetPendingInvitesByUserIdAsync(currentUser.Id);
        var response = invites.Select(i => MapToResponse(i));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetInviteById(
        Guid id,
        ISessionInviteRepository inviteRepository)
    {
        var invite = await inviteRepository.GetByIdAsync(id);

        if (invite == null)
        {
            return Results.NotFound(new { message = "Invite not found" });
        }

        return Results.Ok(MapToResponse(invite));
    }

    private static async Task<IResult> CreateInvite(
        CreateSessionInviteRequest request,
        ClaimsPrincipal user,
        ISessionInviteRepository inviteRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var invite = new SessionInvite
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            UserId = request.UserId,
            Status = "pending",
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await inviteRepository.CreateAsync(invite);
        return Results.Created($"/api/session-invites/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateInvite(
        Guid id,
        UpdateSessionInviteRequest request,
        ClaimsPrincipal user,
        ISessionInviteRepository inviteRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var invite = await inviteRepository.GetByIdAsync(id);
        if (invite == null)
        {
            return Results.NotFound(new { message = "Invite not found" });
        }

        if (invite.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        invite.Status = request.Status;
        invite.UpdatedById = currentUser.Id;
        invite.UpdatedAt = DateTime.UtcNow;

        var updated = await inviteRepository.UpdateAsync(invite);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteInvite(
        Guid id,
        ClaimsPrincipal user,
        ISessionInviteRepository inviteRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var invite = await inviteRepository.GetByIdAsync(id);
        if (invite == null)
        {
            return Results.NotFound(new { message = "Invite not found" });
        }

        if (invite.CreatedById != currentUser.Id && invite.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await inviteRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete invite");
        }

        return Results.NoContent();
    }

    private static SessionInviteResponse MapToResponse(SessionInvite invite)
    {
        return new SessionInviteResponse
        {
            Id = invite.Id,
            SessionId = invite.SessionId,
            UserId = invite.UserId,
            User = invite.User != null ? MapUserToResponse(invite.User) : null,
            Status = invite.Status,
            CreatedAt = invite.CreatedAt,
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
