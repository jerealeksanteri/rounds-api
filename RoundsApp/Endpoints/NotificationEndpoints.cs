// <copyright file="NotificationEndpoints.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RoundsApp.DTOs.Notifications;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var notificationApi = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        notificationApi.MapGet("/", GetMyNotifications)
            .WithName("GetMyNotifications")
            .WithOpenApi();

        notificationApi.MapGet("/unread", GetUnreadNotifications)
            .WithName("GetUnreadNotifications")
            .WithOpenApi();

        notificationApi.MapGet("/{id:guid}", GetNotificationById)
            .WithName("GetNotificationById")
            .WithOpenApi();

        notificationApi.MapPost("/", CreateNotification)
            .WithName("CreateNotification")
            .WithOpenApi();

        notificationApi.MapPut("/{id:guid}/read", MarkAsRead)
            .WithName("MarkNotificationAsRead")
            .WithOpenApi();

        notificationApi.MapPut("/read-all", MarkAllAsRead)
            .WithName("MarkAllNotificationsAsRead")
            .WithOpenApi();

        notificationApi.MapDelete("/{id:guid}", DeleteNotification)
            .WithName("DeleteNotification")
            .WithOpenApi();
    }

    private static async Task<IResult> GetMyNotifications(
        ClaimsPrincipal user,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var notifications = await notificationRepository.GetByUserIdAsync(currentUser.Id);
        var response = notifications.Select(n => MapToResponse(n));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetUnreadNotifications(
        ClaimsPrincipal user,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var notifications = await notificationRepository.GetUnreadByUserIdAsync(currentUser.Id);
        var response = notifications.Select(n => MapToResponse(n));
        return Results.Ok(response);
    }

    private static async Task<IResult> GetNotificationById(
        Guid id,
        ClaimsPrincipal user,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var notification = await notificationRepository.GetByIdAsync(id);

        if (notification == null)
        {
            return Results.NotFound(new { message = "Notification not found" });
        }

        if (notification.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        return Results.Ok(MapToResponse(notification));
    }

    private static async Task<IResult> CreateNotification(
        CreateNotificationRequest request,
        INotificationRepository notificationRepository)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Metadata = request.Metadata,
            Read = false,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await notificationRepository.CreateAsync(notification);
        return Results.Created($"/api/notifications/{created.Id}", MapToResponse(created));
    }

    private static async Task<IResult> MarkAsRead(
        Guid id,
        ClaimsPrincipal user,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var notification = await notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return Results.NotFound(new { message = "Notification not found" });
        }

        if (notification.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var marked = await notificationRepository.MarkAsReadAsync(id);
        if (!marked)
        {
            return Results.Problem("Failed to mark notification as read");
        }

        return Results.Ok(new { message = "Notification marked as read" });
    }

    private static async Task<IResult> MarkAllAsRead(
        ClaimsPrincipal user,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var marked = await notificationRepository.MarkAllAsReadAsync(currentUser.Id);
        if (!marked)
        {
            return Results.Problem("Failed to mark all notifications as read");
        }

        return Results.Ok(new { message = "All notifications marked as read" });
    }

    private static async Task<IResult> DeleteNotification(
        Guid id,
        ClaimsPrincipal user,
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var notification = await notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return Results.NotFound(new { message = "Notification not found" });
        }

        if (notification.UserId != currentUser.Id)
        {
            return Results.Forbid();
        }

        var deleted = await notificationRepository.DeleteAsync(id);
        if (!deleted)
        {
            return Results.Problem("Failed to delete notification");
        }

        return Results.NoContent();
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Metadata = notification.Metadata,
            Read = notification.Read,
            CreatedAt = notification.CreatedAt,
        };
    }
}
