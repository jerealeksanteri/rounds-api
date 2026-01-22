// Copyright (c) 2026 Jere Niemi. All rights reserved.
using RoundsApp.DTOs.Notifications;

namespace RoundsApp.Services;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, NotificationResponse notification);

    Task SendNotificationToMultipleAsync(IEnumerable<Guid> userIds, NotificationResponse notification);

    Task CreateAndSendAsync(Guid userId, string type, string title, string body, string? metadata = null);
}
