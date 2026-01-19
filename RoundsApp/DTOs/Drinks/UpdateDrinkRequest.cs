// <copyright file="UpdateDrinkRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Drinks;

public class UpdateDrinkRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public Guid? DrinkTypeId { get; set; }

    public decimal? AlcoholContent { get; set; }

    public decimal? VolumeLitres { get; set; }
}
