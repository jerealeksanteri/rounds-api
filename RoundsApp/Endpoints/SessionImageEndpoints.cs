// <copyright file="SessionImageEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class SessionImageEndpoints
{
    public static void MapSessionImageEndpoints(this IEndpointRouteBuilder app)
    {
        var imageApi = app.MapGroup("/api/session-images")
            .WithTags("Session Images")
            .RequireAuthorization();

        imageApi.MapGet("/session/{sessionId:guid}", GetImagesBySession)
            .WithName("GetImagesBySession")
            .WithOpenApi();

        imageApi.MapGet("/{id:guid}", GetImageById)
            .WithName("GetImageById")
            .WithOpenApi();

        imageApi.MapPost("/", CreateImage)
            .WithName("CreateImage")
            .WithOpenApi();

        imageApi.MapPut("/{id:guid}", UpdateImage)
            .WithName("UpdateImage")
            .WithOpenApi();

        imageApi.MapDelete("/{id:guid}", DeleteImage)
            .WithName("DeleteImage")
            .WithOpenApi();
    }

    private static async Task<IResult> GetImagesBySession(
        Guid sessionId,
        ISessionImageRepository imageRepository)
    {
        var images = await imageRepository.GetBySessionIdAsync(sessionId);
        var response = images.Select(i => MapToResponse(i));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetImageById(
        Guid id,
        ISessionImageRepository imageRepository)
    {
        var image = await imageRepository.GetByIdAsync(id);

        if (image == null)
        {
            return Results.NotFound(new { message = "Image not found" });
        }

        return Results.Ok(MapToResponse(image));
    }

    private static async Task<IResult> CreateImage(
        CreateSessionImageRequest request,
        ClaimsPrincipal user,
        ISessionImageRepository imageRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var image = new SessionImage
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            Url = request.Url,
            Caption = request.Caption,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await imageRepository.CreateAsync(image);
        return Results.Created($"/api/session-images/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateImage(
        Guid id,
        UpdateSessionImageRequest request,
        ClaimsPrincipal user,
        ISessionImageRepository imageRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var image = await imageRepository.GetByIdAsync(id);
        if (image == null)
        {
            return Results.NotFound(new { message = "Image not found" });
        }

        if (image.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        if (request.Url != null)
        {
            image.Url = request.Url;
        }

        if (request.Caption != null)
        {
            image.Caption = request.Caption;
        }

        image.UpdatedById = currentUser.Id;
        image.UpdatedAt = DateTime.UtcNow;

        var updated = await imageRepository.UpdateAsync(image);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteImage(
        Guid id,
        ClaimsPrincipal user,
        ISessionImageRepository imageRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var image = await imageRepository.GetByIdAsync(id);
        if (image == null)
        {
            return Results.NotFound(new { message = "Image not found" });
        }

        if (image.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await imageRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete image");
        }

        return Results.NoContent();
    }

    private static SessionImageResponse MapToResponse(SessionImage image)
    {
        return new SessionImageResponse
        {
            Id = image.Id,
            SessionId = image.SessionId,
            Url = image.Url,
            Caption = image.Caption,
            CreatedAt = image.CreatedAt,
        };
    }
}
