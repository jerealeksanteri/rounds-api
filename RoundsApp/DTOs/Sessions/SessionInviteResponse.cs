// <copyright file="SessionInviteResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Users;

namespace RoundsApp.DTOs.Sessions;

public class SessionInviteResponse
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public UserResponse? User { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
