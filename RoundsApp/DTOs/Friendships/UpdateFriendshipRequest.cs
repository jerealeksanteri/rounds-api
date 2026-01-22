// <copyright file="UpdateFriendshipRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.DTOs.Friendships;

public class UpdateFriendshipRequest
{
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
}
