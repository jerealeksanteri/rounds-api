// <copyright file="AddGroupMembersRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace RoundsApp.DTOs.FriendGroups;

public class AddGroupMembersRequest
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();
}
