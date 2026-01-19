// <copyright file="SessionLocationResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class SessionLocationResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }
}
