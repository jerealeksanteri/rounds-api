// <copyright file="FriendGroupEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.FriendGroups;
using RoundsApp.DTOs.Users;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;
using RoundsApp.Services;

namespace RoundsApp.Endpoints;

public static class FriendGroupEndpoints
{
    public static void MapFriendGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var groupApi = app.MapGroup("/api/friend-groups")
            .WithTags("Friend Groups")
            .RequireAuthorization();

        groupApi.MapGet("/", GetMyGroups)
            .WithName("GetMyGroups")
            .WithOpenApi();

        groupApi.MapGet("/{id:guid}", GetGroupById)
            .WithName("GetGroupById")
            .WithOpenApi();

        groupApi.MapPost("/", CreateGroup)
            .WithName("CreateGroup")
            .WithOpenApi();

        groupApi.MapPut("/{id:guid}", UpdateGroup)
            .WithName("UpdateGroup")
            .WithOpenApi();

        groupApi.MapDelete("/{id:guid}", DeleteGroup)
            .WithName("DeleteGroup")
            .WithOpenApi();

        groupApi.MapPost("/{id:guid}/members", AddMembers)
            .WithName("AddGroupMembers")
            .WithOpenApi();

        groupApi.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMember)
            .WithName("RemoveGroupMember")
            .WithOpenApi();

        groupApi.MapPost("/{id:guid}/invite-to-session", BulkInviteToSession)
            .WithName("BulkInviteToSession")
            .WithOpenApi();
    }

    private static async Task<IResult> GetMyGroups(
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var groups = await groupRepository.GetByOwnerIdAsync(currentUser.Id);
        var response = groups.Select(g => MapToResponse(g));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetGroupById(
        Guid id,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var group = await groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return Results.NotFound(new { message = "Group not found" });
        }

        if (group.OwnerId != currentUser.Id)
        {
            return Results.Forbid();
        }

        return Results.Ok(MapToResponse(group));
    }

    private static async Task<IResult> CreateGroup(
        CreateFriendGroupRequest request,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        IFriendGroupMemberRepository memberRepository,
        IFriendGroupValidationService validationService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        // Validate initial members are friends
        if (request.InitialMemberIds?.Any() == true)
        {
            var nonFriends = await validationService.FilterNonFriendsAsync(
                currentUser.Id, request.InitialMemberIds);
            if (nonFriends.Any())
            {
                return Results.BadRequest(new
                {
                    message = "Some users are not your friends",
                    nonFriendIds = nonFriends,
                });
            }
        }

        var group = new FriendGroup
        {
            Id = Guid.NewGuid(),
            OwnerId = currentUser.Id,
            Name = request.Name,
            Description = request.Description,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await groupRepository.CreateAsync(group);

        // Add initial members
        if (request.InitialMemberIds?.Any() == true)
        {
            var members = request.InitialMemberIds.Select(userId => new FriendGroupMember
            {
                GroupId = created.Id,
                UserId = userId,
                AddedById = currentUser.Id,
                AddedAt = DateTime.UtcNow,
            });
            await memberRepository.CreateMultipleAsync(members);
        }

        // Reload to get members
        var groupWithMembers = await groupRepository.GetByIdAsync(created.Id);
        return Results.Created($"/api/friend-groups/{created.Id}", MapToResponse(groupWithMembers!));
    }

    private static async Task<IResult> UpdateGroup(
        Guid id,
        UpdateFriendGroupRequest request,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var group = await groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return Results.NotFound(new { message = "Group not found" });
        }

        if (group.OwnerId != currentUser.Id)
        {
            return Results.Forbid();
        }

        if (request.Name != null)
        {
            group.Name = request.Name;
        }

        if (request.Description != null)
        {
            group.Description = request.Description;
        }

        group.UpdatedById = currentUser.Id;
        group.UpdatedAt = DateTime.UtcNow;

        var updated = await groupRepository.UpdateAsync(group);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteGroup(
        Guid id,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var group = await groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return Results.NotFound(new { message = "Group not found" });
        }

        if (group.OwnerId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await groupRepository.DeleteAsync(group);
        if (!deleted)
        {
            return Results.Problem("Failed to delete group");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> AddMembers(
        Guid id,
        AddGroupMembersRequest request,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        IFriendGroupMemberRepository memberRepository,
        IFriendGroupValidationService validationService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var group = await groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return Results.NotFound(new { message = "Group not found" });
        }

        if (group.OwnerId != currentUser.Id)
        {
            return Results.Forbid();
        }

        // Validate all users are friends
        var nonFriends = await validationService.FilterNonFriendsAsync(currentUser.Id, request.UserIds);
        if (nonFriends.Any())
        {
            return Results.BadRequest(new
            {
                message = "Some users are not your friends",
                nonFriendIds = nonFriends,
            });
        }

        // Filter out users who are already members
        var existingMemberIds = group.Members.Select(m => m.UserId).ToHashSet();
        var newUserIds = request.UserIds.Where(uid => !existingMemberIds.Contains(uid)).ToList();

        if (newUserIds.Count == 0)
        {
            return Results.BadRequest(new { message = "All users are already members of this group" });
        }

        var members = newUserIds.Select(userId => new FriendGroupMember
        {
            GroupId = id,
            UserId = userId,
            AddedById = currentUser.Id,
            AddedAt = DateTime.UtcNow,
        });

        await memberRepository.CreateMultipleAsync(members);

        // Reload group with updated members
        var updatedGroup = await groupRepository.GetByIdAsync(id);
        return Results.Ok(MapToResponse(updatedGroup!));
    }

    private static async Task<IResult> RemoveMember(
        Guid id,
        Guid userId,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        IFriendGroupMemberRepository memberRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var group = await groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return Results.NotFound(new { message = "Group not found" });
        }

        if (group.OwnerId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await memberRepository.DeleteAsync(id, userId);
        if (!deleted)
        {
            return Results.NotFound(new { message = "Member not found in group" });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> BulkInviteToSession(
        Guid id,
        BulkInviteToSessionRequest request,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        IFriendGroupMemberRepository memberRepository,
        ISessionInviteRepository inviteRepository,
        IDrinkingSessionRepository sessionRepository,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var group = await groupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return Results.NotFound(new { message = "Group not found" });
        }

        if (group.OwnerId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var session = await sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null)
        {
            return Results.NotFound(new { message = "Session not found" });
        }

        var members = await memberRepository.GetByGroupIdAsync(id);
        var createdInvites = new List<SessionInvite>();

#pragma warning disable S3267 // Loop has side effects (creating invites and sending notifications)
        foreach (var member in members)
#pragma warning restore S3267
        {
            var invite = new SessionInvite
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                UserId = member.UserId,
                Status = "pending",
                CreatedById = currentUser.Id,
                CreatedAt = DateTime.UtcNow,
            };

            var created = await inviteRepository.CreateAsync(invite);
            createdInvites.Add(created);

            await notificationService.CreateAndSendAsync(
                member.UserId,
                "session_invite",
                "New Session Invite",
                $"{currentUser.UserName} invited you to {session.Name}",
                JsonSerializer.Serialize(new { sessionId = session.Id, inviteId = created.Id }));
        }

        return Results.Ok(new
        {
            invitesSent = createdInvites.Count,
            groupName = group.Name,
            sessionName = session.Name,
        });
    }

    private static FriendGroupResponse MapToResponse(FriendGroup group)
    {
        return new FriendGroupResponse
        {
            Id = group.Id,
            OwnerId = group.OwnerId,
            Name = group.Name,
            Description = group.Description,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            MemberCount = group.Members.Count,
            Members = group.Members.Select(m => MapMemberToResponse(m)).ToList(),
        };
    }

    private static FriendGroupMemberResponse MapMemberToResponse(FriendGroupMember member)
    {
        return new FriendGroupMemberResponse
        {
            GroupId = member.GroupId,
            UserId = member.UserId,
            User = member.User != null ? MapUserToResponse(member.User) : null,
            AddedAt = member.AddedAt,
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
