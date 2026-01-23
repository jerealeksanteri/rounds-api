// <copyright file="FriendGroupResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.FriendGroups;

public class FriendGroupResponse
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int MemberCount { get; set; }

    public List<FriendGroupMemberResponse> Members { get; set; } = new();
}
