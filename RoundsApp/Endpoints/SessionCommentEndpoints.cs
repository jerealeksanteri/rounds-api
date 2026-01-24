// <copyright file="SessionCommentEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.DTOs.Users;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;
using RoundsApp.Services;

namespace RoundsApp.Endpoints;

public static class SessionCommentEndpoints
{
    public static void MapSessionCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var commentApi = app.MapGroup("/api/session-comments")
            .WithTags("Session Comments")
            .RequireAuthorization();

        commentApi.MapGet("/session/{sessionId:guid}", GetCommentsBySession)
            .WithName("GetCommentsBySession")
            .WithOpenApi();

        commentApi.MapGet("/{id:guid}", GetCommentById)
            .WithName("GetCommentById")
            .WithOpenApi();

        commentApi.MapPost("/", CreateComment)
            .WithName("CreateComment")
            .WithOpenApi();

        commentApi.MapPut("/{id:guid}", UpdateComment)
            .WithName("UpdateComment")
            .WithOpenApi();

        commentApi.MapDelete("/{id:guid}", DeleteComment)
            .WithName("DeleteComment")
            .WithOpenApi();
    }

    private static async Task<IResult> GetCommentsBySession(
        Guid sessionId,
        ISessionCommentRepository commentRepository)
    {
        var comments = await commentRepository.GetBySessionIdAsync(sessionId);
        var response = comments.Select(c => MapToResponse(c));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCommentById(
        Guid id,
        ISessionCommentRepository commentRepository)
    {
        var comment = await commentRepository.GetByIdAsync(id);

        if (comment == null)
        {
            return Results.NotFound(new { message = "Comment not found" });
        }

        return Results.Ok(MapToResponse(comment));
    }

    private static async Task<IResult> CreateComment(
        CreateCommentRequest request,
        ClaimsPrincipal user,
        ISessionCommentRepository commentRepository,
        IDrinkingSessionRepository sessionRepository,
        IMentionService mentionService,
        INotificationService notificationService,
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

        var comment = new SessionComment
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            UserId = currentUser.Id,
            Content = request.Content,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await commentRepository.CreateAsync(comment);

        // Parse and create mentions
        var mentions = await mentionService.ParseAndCreateMentionsAsync(
            created.Id,
            created.Content,
            currentUser.Id);

        // Send notifications to mentioned users (excluding self-mentions)
        foreach (var mention in mentions)
        {
            if (mention.MentionedUserId != currentUser.Id)
            {
                await notificationService.CreateAndSendAsync(
                    mention.MentionedUserId,
                    "mention",
                    "You were mentioned",
                    $"{currentUser.UserName} mentioned you in a comment",
                    JsonSerializer.Serialize(new
                    {
                        commentId = created.Id,
                        sessionId = request.SessionId,
                        sessionName = session.Name,
                    }));
            }
        }

        return Results.Created($"/api/session-comments/{created.Id}", MapToResponse(created, mentions));
    }

    private static async Task<IResult> UpdateComment(
        Guid id,
        UpdateCommentRequest request,
        ClaimsPrincipal user,
        ISessionCommentRepository commentRepository,
        ICommentMentionRepository mentionRepository,
        IDrinkingSessionRepository sessionRepository,
        IMentionService mentionService,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var comment = await commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            return Results.NotFound(new { message = "Comment not found" });
        }

        if (comment.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        // Get old mentions for comparison
        var oldMentions = await mentionRepository.GetByCommentIdAsync(id);
        var oldMentionedUserIds = oldMentions.Select(m => m.MentionedUserId).ToHashSet();

        comment.Content = request.Content;
        comment.UpdatedById = currentUser.Id;
        comment.UpdatedAt = DateTime.UtcNow;

        var updated = await commentRepository.UpdateAsync(comment);

        // Delete old mentions and create new ones
        await mentionRepository.DeleteByCommentIdAsync(id);
        var newMentions = await mentionService.ParseAndCreateMentionsAsync(
            id,
            updated.Content,
            currentUser.Id);

        // Send notifications to newly mentioned users only
        var session = await sessionRepository.GetByIdAsync(comment.SessionId);
        foreach (var mention in newMentions)
        {
            if (mention.MentionedUserId != currentUser.Id &&
                !oldMentionedUserIds.Contains(mention.MentionedUserId))
            {
                await notificationService.CreateAndSendAsync(
                    mention.MentionedUserId,
                    "mention",
                    "You were mentioned",
                    $"{currentUser.UserName} mentioned you in a comment",
                    JsonSerializer.Serialize(new
                    {
                        commentId = id,
                        sessionId = comment.SessionId,
                        sessionName = session?.Name,
                    }));
            }
        }

        return Results.Ok(MapToResponse(updated, newMentions));
    }

    private static async Task<IResult> DeleteComment(
        Guid id,
        ClaimsPrincipal user,
        ISessionCommentRepository commentRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var comment = await commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            return Results.NotFound(new { message = "Comment not found" });
        }

        if (comment.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await commentRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete comment");
        }

        return Results.NoContent();
    }

    private static CommentResponse MapToResponse(SessionComment comment, IEnumerable<CommentMention>? mentions = null)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            SessionId = comment.SessionId,
            UserId = comment.UserId,
            User = comment.User != null ? MapUserToResponse(comment.User) : null,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CreatedById = comment.CreatedById,
            UpdatedAt = comment.UpdatedAt,
            Mentions = mentions?.Select(m => new CommentMentionResponse
            {
                Id = m.Id,
                CommentId = m.CommentId,
                MentionedUserId = m.MentionedUserId,
                MentionedUser = m.MentionedUser != null ? MapUserToResponse(m.MentionedUser) : null,
                StartPosition = m.StartPosition,
                Length = m.Length,
            }).ToList() ??[],
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
