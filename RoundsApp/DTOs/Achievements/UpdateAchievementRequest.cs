// <copyright file="UpdateAchievementRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Achievements;

public class UpdateAchievementRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public string? Icon { get; set; }

    public string? Criteria { get; set; }
}
