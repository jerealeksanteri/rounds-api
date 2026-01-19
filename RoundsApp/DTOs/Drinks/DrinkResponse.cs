// <copyright file="DrinkResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Users;

namespace RoundsApp.DTOs.Drinks;

public class DrinkResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DrinkTypeId { get; set; }

    public DrinkTypeResponse? DrinkType { get; set; }

    public decimal AlcoholContent { get; set; }

    public decimal VolumeLitres { get; set; }

    public Guid CreatedById { get; set; }

    public UserResponse? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<DrinkImageResponse> Images { get; set; } = new();
}
