// <copyright file="FriendshipEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Friendships;
using RoundsApp.DTOs.Users;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;
using RoundsApp.Services;

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
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var friend = await userManager.FindByIdAsync(request.FriendId.ToString());
        if (friend == null)
        {
            return Results.NotFound(new { message = "Friend not found" });
        }

        // Query for existing records
        var existingFriendship = await friendshipRepository.GetByIdAsync(currentUser.Id, request.FriendId);
        var existingBidirection = await friendshipRepository.GetByIdAsync(request.FriendId, currentUser.Id);

        // Check is there is existing relation between users which is not rejected.
        var isExistingFriendship = existingFriendship is { Status: not FriendshipStatus.Rejected };
        var isExistingBidirection = existingBidirection is { Status: not FriendshipStatus.Rejected };

        var isRelationExisting = isExistingFriendship || isExistingBidirection;

        if (isRelationExisting)
        {
            return Results.BadRequest(new { message = "Friendship relation already exists." });
        }

        var friendship = new Friendship
        {
            UserId = currentUser.Id,
            FriendId = request.FriendId,
            Status = FriendshipStatus.Pending,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        // Send notification to friend
        await notificationService.CreateAndSendAsync(
            request.FriendId,
            "friend_request",
            "New Friend Request",
            $"{currentUser.UserName} sent you a friend request",
            JsonSerializer.Serialize(new { userId = currentUser.Id }));

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

        // Only the recipient (FriendId) can accept/reject, but sender can cancel (delete)
        if (friendship.FriendId != currentUser.Id)
        {
            return Results.Forbid();
        }

        friendship.Status = request.Status;
        friendship.UpdatedById = currentUser.Id;
        friendship.UpdatedAt = DateTime.UtcNow;

        // Update the original friendship first
        var updated = await friendshipRepository.UpdateAsync(friendship);

        // If accepted, also create the bi-directional entry
        if (request.Status == FriendshipStatus.Accepted)
        {
            var bidirectionalFriendship = new Friendship
            {
                UserId = friendship.FriendId,
                FriendId = friendship.UserId,
                Status = FriendshipStatus.Accepted,
                CreatedById = currentUser.Id,
                CreatedAt = DateTime.UtcNow,
            };

            await friendshipRepository.CreateAsync(bidirectionalFriendship);
        }

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

        // Delete also the bi-direction (if it exists - only accepted friendships have bidirectional entries)
#pragma warning disable S2234 // Arguments should be passed in the correct order - intentionally swapped for bidirectional delete
        await friendshipRepository.DeleteAsync(friendId, userId);
#pragma warning restore S2234

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
