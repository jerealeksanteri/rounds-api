// <copyright file="DrinkImageResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Drinks;

public class DrinkImageResponse
{
    public Guid Id { get; set; }

    public Guid DrinkId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public DateTime CreatedAt { get; set; }
}
