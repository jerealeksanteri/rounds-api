// <copyright file="CreateSessionRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class CreateSessionRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public Guid? LocationId { get; set; }
}
