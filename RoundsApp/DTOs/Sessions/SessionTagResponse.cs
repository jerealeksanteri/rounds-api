// <copyright file="SessionTagResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class SessionTagResponse
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public string Tag { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
