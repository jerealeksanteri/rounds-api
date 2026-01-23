// <copyright file="FriendGroupMemberResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Users;

namespace RoundsApp.DTOs.FriendGroups;

public class FriendGroupMemberResponse
{
    public Guid GroupId { get; set; }

    public Guid UserId { get; set; }

    public UserResponse? User { get; set; }

    public DateTime AddedAt { get; set; }
}
