// <copyright file="FriendshipResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Users;
using RoundsApp.Models;

namespace RoundsApp.DTOs.Friendships;

public class FriendshipResponse
{
    public Guid UserId { get; set; }

    public UserResponse? User { get; set; }

    public Guid FriendId { get; set; }

    public UserResponse? Friend { get; set; }

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

    public DateTime CreatedAt { get; set; }

    public Guid CreatedById { get; set; }
}
