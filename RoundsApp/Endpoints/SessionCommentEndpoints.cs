// <copyright file="SessionCommentEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

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
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
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
        return Results.Created($"/api/session-comments/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateComment(
        Guid id,
        UpdateCommentRequest request,
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

        comment.Content = request.Content;
        comment.UpdatedById = currentUser.Id;
        comment.UpdatedAt = DateTime.UtcNow;

        var updated = await commentRepository.UpdateAsync(comment);
        return Results.Ok(MapToResponse(updated));
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

    private static CommentResponse MapToResponse(SessionComment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            SessionId = comment.SessionId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CreatedById = comment.CreatedById,
            UpdatedAt = comment.UpdatedAt,
        };
    }
}
