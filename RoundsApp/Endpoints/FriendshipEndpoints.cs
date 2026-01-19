// <copyright file="FriendshipEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Friendships;
using RoundsApp.DTOs.Users;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class FriendshipEndpoints
{
    public static void MapFriendshipEndpoints(this IEndpointRouteBuilder app)
    {
        var friendshipApi = app.MapGroup("/api/friendships")
            .WithTags("Friendships")
            .RequireAuthorization();

        friendshipApi.MapGet("/", GetAllFriendships)
            .WithName("GetAllFriendships")
            .WithOpenApi();

        friendshipApi.MapGet("/my-friends", GetMyFriends)
            .WithName("GetMyFriends")
            .WithOpenApi();

        friendshipApi.MapGet("/pending", GetPendingRequests)
            .WithName("GetPendingRequests")
            .WithOpenApi();

        friendshipApi.MapGet("/sent", GetSentRequests)
            .WithName("GetSentRequests")
            .WithOpenApi();

        friendshipApi.MapPost("/", CreateFriendship)
            .WithName("CreateFriendship")
            .WithOpenApi();

        friendshipApi.MapPut("/{userId:guid}/{friendId:guid}", UpdateFriendship)
            .WithName("UpdateFriendship")
            .WithOpenApi();

        friendshipApi.MapDelete("/{userId:guid}/{friendId:guid}", DeleteFriendship)
            .WithName("DeleteFriendship")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllFriendships(
        IFriendshipRepository friendshipRepository)
    {
        var friendships = await friendshipRepository.GetAllAsync();
        var response = friendships.Select(f => MapToResponse(f));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetMyFriends(
        ClaimsPrincipal user,
        IFriendshipRepository friendshipRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var friends = await friendshipRepository.GetFriendsByUserIdAsync(currentUser.Id);
        var response = friends.Select(f => MapToResponse(f));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetPendingRequests(
        ClaimsPrincipal user,
        IFriendshipRepository friendshipRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var pending = await friendshipRepository.GetPendingRequestsByUserIdAsync(currentUser.Id);
        var response = pending.Select(f => MapToResponse(f));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetSentRequests(
        ClaimsPrincipal user,
        IFriendshipRepository friendshipRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var sent = await friendshipRepository.GetSentRequestsByUserIdAsync(currentUser.Id);
        var response = sent.Select(f => MapToResponse(f));
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateFriendship(
        CreateFriendshipRequest request,
        ClaimsPrincipal user,
        IFriendshipRepository friendshipRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var friendship = new Friendship
        {
            UserId = currentUser.Id,
            FriendId = request.FriendId,
            Status = "pending",
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await friendshipRepository.CreateAsync(friendship);
        return Results.Created($"/api/friendships/{created.UserId}/{created.FriendId}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateFriendship(
        Guid userId,
        Guid friendId,
        UpdateFriendshipRequest request,
        ClaimsPrincipal user,
        IFriendshipRepository friendshipRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var friendship = await friendshipRepository.GetByIdAsync(userId, friendId);
        if (friendship == null)
        {
            return Results.NotFound(new { message = "Friendship not found" });
        }

        if (friendship.FriendId != currentUser.Id && friendship.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        friendship.Status = request.Status;
        friendship.UpdatedById = currentUser.Id;
        friendship.UpdatedAt = DateTime.UtcNow;

        var updated = await friendshipRepository.UpdateAsync(friendship);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteFriendship(
        Guid userId,
        Guid friendId,
        ClaimsPrincipal user,
        IFriendshipRepository friendshipRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var friendship = await friendshipRepository.GetByIdAsync(userId, friendId);
        if (friendship == null)
        {
            return Results.NotFound(new { message = "Friendship not found" });
        }

        if (friendship.FriendId != currentUser.Id && friendship.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await friendshipRepository.DeleteAsync(userId, friendId);
        if (!deleted)
        {
            return Results.Problem("Failed to delete friendship");
        }

        return Results.NoContent();
    }

    private static FriendshipResponse MapToResponse(Friendship friendship)
    {
        return new FriendshipResponse
        {
            UserId = friendship.UserId,
            User = friendship.User != null ? MapUserToResponse(friendship.User) : null,
            FriendId = friendship.FriendId,
            Friend = friendship.Friend != null ? MapUserToResponse(friendship.Friend) : null,
            Status = friendship.Status,
            CreatedAt = friendship.CreatedAt,
            CreatedById = friendship.CreatedById,
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
