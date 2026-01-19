// <copyright file="NotificationResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Metadata { get; set; }

    public bool Read { get; set; }

    public DateTime CreatedAt { get; set; }
}
