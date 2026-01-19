// <copyright file="CreateDrinkTypeRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Drinks;

public class CreateDrinkTypeRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
