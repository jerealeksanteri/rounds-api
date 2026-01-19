// <copyright file="SessionLocationEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Sessions;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class SessionLocationEndpoints
{
    public static void MapSessionLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var locationApi = app.MapGroup("/api/session-locations")
            .WithTags("Session Locations")
            .RequireAuthorization();

        locationApi.MapGet("/", GetAllLocations)
            .WithName("GetAllLocations")
            .WithOpenApi();

        locationApi.MapGet("/search", SearchLocations)
            .WithName("SearchLocations")
            .WithOpenApi();

        locationApi.MapGet("/{id:guid}", GetLocationById)
            .WithName("GetLocationById")
            .WithOpenApi();

        locationApi.MapPost("/", CreateLocation)
            .WithName("CreateLocation")
            .WithOpenApi();

        locationApi.MapPut("/{id:guid}", UpdateLocation)
            .WithName("UpdateLocation")
            .WithOpenApi();

        locationApi.MapDelete("/{id:guid}", DeleteLocation)
            .WithName("DeleteLocation")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllLocations(
        ISessionLocationRepository locationRepository)
    {
        var locations = await locationRepository.GetAllAsync();
        var response = locations.Select(l => MapToResponse(l));
        return Results.Ok(response);
    }

    private static async Task<IResult> SearchLocations(
        string name,
        ISessionLocationRepository locationRepository)
    {
        var locations = await locationRepository.SearchByNameAsync(name);
        var response = locations.Select(l => MapToResponse(l));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetLocationById(
        Guid id,
        ISessionLocationRepository locationRepository)
    {
        var location = await locationRepository.GetByIdAsync(id);

        if (location == null)
        {
            return Results.NotFound(new { message = "Location not found" });
        }

        return Results.Ok(MapToResponse(location));
    }

    private static async Task<IResult> CreateLocation(
        CreateSessionLocationRequest request,
        ClaimsPrincipal user,
        ISessionLocationRepository locationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var location = new SessionLocation
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await locationRepository.CreateAsync(location);
        return Results.Created($"/api/session-locations/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> UpdateLocation(
        Guid id,
        UpdateSessionLocationRequest request,
        ClaimsPrincipal user,
        ISessionLocationRepository locationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var location = await locationRepository.GetByIdAsync(id);
        if (location == null)
        {
            return Results.NotFound(new { message = "Location not found" });
        }

        if (location.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        if (request.Name != null)
        {
            location.Name = request.Name;
        }

        if (request.Address != null)
        {
            location.Address = request.Address;
        }

        if (request.Latitude.HasValue)
        {
            location.Latitude = request.Latitude;
        }

        if (request.Longitude.HasValue)
        {
            location.Longitude = request.Longitude;
        }

        location.UpdatedById = currentUser.Id;
        location.UpdatedAt = DateTime.UtcNow;

        var updated = await locationRepository.UpdateAsync(location);
        return Results.Ok(MapToResponse(updated));
    }

    private static async Task<IResult> DeleteLocation(
        Guid id,
        ClaimsPrincipal user,
        ISessionLocationRepository locationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var location = await locationRepository.GetByIdAsync(id);
        if (location == null)
        {
            return Results.NotFound(new { message = "Location not found" });
        }

        if (location.CreatedById != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await locationRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete location");
        }

        return Results.NoContent();
    }

    private static SessionLocationResponse MapToResponse(SessionLocation location)
    {
        return new SessionLocationResponse
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            CreatedAt = location.CreatedAt,
        };
    }
}
