// <copyright file="DrinkingSessionEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs;
using RoundsApp.Models;
using RoundsApp.Services;

namespace RoundsApp.Endpoints;

public static class DrinkingSessionEndpoints
{
    public static void MapDrinkingSessionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder dsApi = app.MapGroup("/api/drinkingsessions")
            .WithTags("DrinkingSessions")
            .RequireAuthorization();

        dsApi.MapPost("/", CreateDrinkingSession)
            .WithName("Create Drinking Session")
            .WithOpenApi();

        dsApi.MapPut("/{sessionId:guid}", UpdateDrinkingSession)
            .WithName("Update Drinking Session")
            .WithOpenApi();

        dsApi.MapDelete("/{sessionId:guid}", DeleteDrinkingSession)
            .WithName("Delete Drinking Session")
            .WithOpenApi();

        dsApi.MapGet("/users/{userId:guid}", GetDrinkingSessionsByUserId)
            .WithName("Get Drinking Sessions By User Id")
            .WithOpenApi();

        dsApi.MapGet("/{sessionId:guid}/participants", GetParticipants)
            .WithName("Get Participants")
            .WithOpenApi();

        dsApi.MapPost("/{sessionId:guid}/participants", AddParticipant)
            .WithName("Add Participant")
            .WithOpenApi();

        dsApi.MapDelete("/{sessionId:guid}/participants/{participantId:guid}", RemoveParticipant)
            .WithName("Remove Participant")
            .WithOpenApi();

        dsApi.MapGet("/{sessionId:guid}/images", GetImages)
            .WithName("Get Images")
            .WithOpenApi();

        dsApi.MapPost("/{sessionId:guid}/images", AddImage)
            .WithName("Add Image")
            .WithOpenApi();

        dsApi.MapDelete("/images/{imageId:guid}", RemoveImage)
            .WithName("Remove Image")
            .WithOpenApi();

        dsApi.MapPost("/{sessionId:guid}/drinks", RecordDrink)
            .WithName("Record Drink")
            .WithOpenApi();

        dsApi.MapDelete("/drinks/{sessionParticipationDrinkId:guid}", RemoveDrink)
            .WithName("Remove Drink")
            .WithOpenApi();

        dsApi.MapGet("/{sessionId:guid}/participants/{participantId:guid}/drinks", GetParticipantDrinks)
            .WithName("Get Participant Drinks")
            .WithOpenApi();

        dsApi.MapGet("/participants/{userId:guid}", GetDrinkingSessionsByParticipantId)
            .WithName("Get Drinking Sessions By Participant Id")
            .WithOpenApi();

        dsApi.MapGet("/{sessionId:guid}/alldrinks", GetAllDrinksInSession)
            .WithName("Get All Drinks In Session")
            .WithOpenApi();

        dsApi.MapGet("/{sessionId:guid}/totaldrinkcount", GetTotalDrinkCountInSession)
            .WithName("Get Total Drink Count In Session")
            .WithOpenApi();

        dsApi.MapGet("/{sessionId:guid}/drinkcountsperparticipant", GetDrinkCountsPerParticipant)
            .WithName("Get Drink Counts Per Participant")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateDrinkingSession(
        CreateDrinkingSessionRequest request,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        // Get the user ID from the JWT token
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        // If user ID is not found, return unauthorized
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        // Create the drinking session
        var session = await drinkingSessionService.CreateDrinkingSessionAsync(request, userId.Value);
        return Results.Ok(session);
    }

    private static async Task<IResult> UpdateDrinkingSession(
        Guid sessionId,
        CreateDrinkingSessionRequest request,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var session = await drinkingSessionService.UpdateDrinkingSessionAsync(sessionId, request, userId.Value);
            return Results.Ok(session);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(statusCode: 403, detail: ex.Message);
        }
    }

    private static async Task<IResult> DeleteDrinkingSession(
        Guid sessionId,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await drinkingSessionService.DeleteDrinkingSessionAsync(sessionId, userId.Value);
            if (!success)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(statusCode: 403, detail: ex.Message);
        }
    }

    private static async Task<IResult> GetDrinkingSessionsByUserId(
        Guid userId,
        IDrinkingSessionService drinkingSessionService)
    {
        var sessions = await drinkingSessionService.GetDrinkingSessionsByParticipantsIdAsync(userId);
        return Results.Ok(sessions);
    }

    private static async Task<IResult> GetParticipants(
        Guid sessionId,
        IDrinkingSessionService drinkingSessionService)
    {
        var participants = await drinkingSessionService.GetParticipantsAsync(sessionId);
        return Results.Ok(participants);
    }

    private static async Task<IResult> AddParticipant(
        Guid sessionId,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var success = await drinkingSessionService.AddParticipantAsync(sessionId, userId.Value, userId.Value);
        if (!success)
        {
            return Results.BadRequest("Unable to add participant. Session not found or participant already exists.");
        }

        return Results.Ok();
    }

    private static async Task<IResult> RemoveParticipant(
        Guid sessionId,
        Guid participantId,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await drinkingSessionService.RemoveParticipantAsync(sessionId, participantId, userId.Value);
            if (!success)
            {
                return Results.NotFound("Participant not found in this session.");
            }

            return Results.NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(statusCode: 403, detail: ex.Message);
        }
    }

    private static async Task<IResult> GetImages(
        Guid sessionId,
        IDrinkingSessionService drinkingSessionService)
    {
        var images = await drinkingSessionService.GetImagesAsync(sessionId);
        return Results.Ok(images);
    }

    private static async Task<IResult> AddImage(
        Guid sessionId,
        AddImageRequest request,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var success = await drinkingSessionService.AddImageAsync(sessionId, request.ImageData, userId.Value);
        if (!success)
        {
            return Results.NotFound("Session not found.");
        }

        return Results.Ok();
    }

    private static async Task<IResult> RemoveImage(
        Guid imageId,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await drinkingSessionService.DeleteImageAsync(imageId, userId.Value);
            if (!success)
            {
                return Results.NotFound("Image not found.");
            }

            return Results.NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(statusCode: 403, detail: ex.Message);
        }
    }

    private static async Task<IResult> GetParticipantDrinks(
        Guid sessionId,
        Guid participantId,
        IDrinkingSessionService drinkingSessionService)
    {
        var drinks = await drinkingSessionService.GetParticipantDrinksAsync(sessionId, participantId);
        return Results.Ok(drinks);
    }

    private static async Task<IResult> RecordDrink(
        Guid sessionId,
        RecordDrinkRequest request,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var success = await drinkingSessionService.RecordDrinkAsync(sessionId, request.ParticipantId, request.DrinkId, userId.Value);
        if (!success)
        {
            return Results.BadRequest("Unable to record drink. Session, participant, or drink not found.");
        }

        return Results.Ok();
    }

    private static async Task<IResult> RemoveDrink(
        Guid sessionParticipationDrinkId,
        IDrinkingSessionService drinkingSessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        Guid? userId = null;
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.Identity?.Name, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        if (userId == null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await drinkingSessionService.RemoveDrinkAsync(sessionParticipationDrinkId, userId.Value);
            if (!success)
            {
                return Results.NotFound("Drink record not found.");
            }

            return Results.NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(statusCode: 403, detail: ex.Message);
        }
    }

    private static async Task<IResult> GetDrinkingSessionsByParticipantId(
        Guid userId,
        IDrinkingSessionService drinkingSessionService)
    {
        var sessions = await drinkingSessionService.GetDrinkingSessionsByParticipantsIdAsync(userId);
        return Results.Ok(sessions);
    }

    private static async Task<IResult> GetAllDrinksInSession(
        Guid sessionId,
        IDrinkingSessionService drinkingSessionService)
    {
        var drinks = await drinkingSessionService.GetAllDrinksInSessionAsync(sessionId);
        return Results.Ok(drinks);
    }

    private static async Task<IResult> GetTotalDrinkCountInSession(
        Guid sessionId,
        IDrinkingSessionService drinkingSessionService)
    {
        var count = await drinkingSessionService.GetTotalDrinkCountInSessionAsync(sessionId);
        return Results.Ok(count);
    }

    private static async Task<IResult> GetDrinkCountsPerParticipant(
        Guid sessionId,
        IDrinkingSessionService drinkingSessionService)
    {
        var counts = await drinkingSessionService.GetDrinkCountsPerParticipantAsync(sessionId);
        return Results.Ok(counts);
    }
}
