// <copyright file="FriendshipResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Friendships;

public class FriendshipResponse
{
    public Guid UserId { get; set; }

    public Guid FriendId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid CreatedById { get; set; }
}
