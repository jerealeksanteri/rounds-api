// <copyright file="CreateSessionInviteRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class CreateSessionInviteRequest
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }
}
