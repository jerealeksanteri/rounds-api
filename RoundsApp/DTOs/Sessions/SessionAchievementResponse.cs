// <copyright file="SessionAchievementResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Achievements;

namespace RoundsApp.DTOs.Sessions;

public class SessionAchievementResponse
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid AchievementId { get; set; }

    public AchievementResponse? Achievement { get; set; }

    public DateTime UnlockedAt { get; set; }
}
