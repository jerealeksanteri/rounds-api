// <copyright file="UpdateFriendGroupRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace RoundsApp.DTOs.FriendGroups;

public class UpdateFriendGroupRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
