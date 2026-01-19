// <copyright file="CreateParticipantRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class CreateParticipantRequest
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }
}
