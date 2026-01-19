// <copyright file="SessionTagEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class SessionTagEndpoints
{
    public static void MapSessionTagEndpoints(this IEndpointRouteBuilder app)
    {
        var tagApi = app.MapGroup("/api/session-tags")
            .WithTags("Session Tags")
            .RequireAuthorization();

        tagApi.MapGet("/session/{sessionId:guid}", GetTagsBySession)
            .WithName("GetTagsBySession")
            .WithOpenApi();

        tagApi.MapGet("/search", GetTagsByName)
            .WithName("GetTagsByName")
            .WithOpenApi();

        tagApi.MapGet("/{id:guid}", GetTagById)
            .WithName("GetTagById")
            .WithOpenApi();

        tagApi.MapPost("/", CreateTag)
            .WithName("CreateTag")
            .WithOpenApi();

        tagApi.MapDelete("/{id:guid}", DeleteTag)
            .WithName("DeleteTag")
            .WithOpenApi();
    }

    private static async Task<IResult> GetTagsBySession(
        Guid sessionId,
        ISessionTagRepository tagRepository)
    {
        var tags = await tagRepository.GetBySessionIdAsync(sessionId);
        var response = tags.Select(t => MapToResponse(t));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetTagsByName(
        string name,
        ISessionTagRepository tagRepository)
    {
        var tags = await tagRepository.GetByTagNameAsync(name);
        var response = tags.Select(t => MapToResponse(t));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetTagById(
        Guid id,
        ISessionTagRepository tagRepository)
    {
        var tag = await tagRepository.GetByIdAsync(id);

        if (tag == null)
        {
            return Results.NotFound(new { message = "Tag not found" });
        }

        return Results.Ok(MapToResponse(tag));
    }

    private static async Task<IResult> CreateTag(
        CreateSessionTagRequest request,
        ClaimsPrincipal user,
        ISessionTagRepository tagRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var tag = new SessionTag
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            Tag = request.Tag,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await tagRepository.CreateAsync(tag);
        return Results.Created($"/api/session-tags/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> DeleteTag(
        Guid id,
        ClaimsPrincipal user,
        ISessionTagRepository tagRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var tag = await tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            return Results.NotFound(new { message = "Tag not found" });
        }

        if (tag.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await tagRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete tag");
        }

        return Results.NoContent();
    }

    private static SessionTagResponse MapToResponse(SessionTag tag)
    {
        return new SessionTagResponse
        {
            Id = tag.Id,
            SessionId = tag.SessionId,
            Tag = tag.Tag,
            CreatedAt = tag.CreatedAt,
        };
    }
}
