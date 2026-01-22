// "// Copyright (c) 2026 Jere Niemi. All rights reserved."

using Microsoft.AspNetCore.SignalR;
using RoundsApp.DTOs.Notifications;
using RoundsApp.Hubs;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        INotificationRepository notificationRepository)
    {
        _hubContext = hubContext;
        _notificationRepository = notificationRepository;
    }

    public async Task SendNotificationAsync(Guid userId, NotificationResponse notification)
    {
        await _hubContext.Clients.Group(userId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationToMultipleAsync(IEnumerable<Guid> userIds, NotificationResponse notification)
    {
        var tasks = userIds.Select(userId => SendNotificationAsync(userId, notification));
        await Task.WhenAll(tasks);
    }

    public async Task CreateAndSendAsync(Guid userId, string type, string title, string body, string? metadata = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = body,
            Metadata = metadata,
            Read = false,
            CreatedAt = DateTime.UtcNow,
        };

        var created = await _notificationRepository.CreateAsync(notification);

        var response = new NotificationResponse
        {
            Id = created.Id,
            UserId = created.UserId,
            Type = created.Type,
            Title = created.Title,
            Message = created.Message,
            Metadata = created.Metadata,
            Read = created.Read,
            CreatedAt = created.CreatedAt,
        };

        await SendNotificationAsync(userId, response);
    }
}
