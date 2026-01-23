// <copyright file="BulkInviteToSessionRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace RoundsApp.DTOs.FriendGroups;

public class BulkInviteToSessionRequest
{
    [Required]
    public Guid SessionId { get; set; }
}
