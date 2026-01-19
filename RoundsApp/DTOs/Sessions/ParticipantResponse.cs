// <copyright file="ParticipantResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class ParticipantResponse
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedById { get; set; }
}
