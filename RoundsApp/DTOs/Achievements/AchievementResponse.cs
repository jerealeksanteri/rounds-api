// <copyright file="AchievementResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Achievements;

public class AchievementResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public string Criteria { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }
}
