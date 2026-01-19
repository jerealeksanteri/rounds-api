// <copyright file="SessionImageResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class SessionImageResponse
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public DateTime CreatedAt { get; set; }
}
