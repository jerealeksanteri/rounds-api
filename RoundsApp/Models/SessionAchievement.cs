// <copyright file="SessionAchievement.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoundsApp.Models;

public class SessionAchievement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public DrinkingSession? Session { get; set; }

    [Required]
    public Guid AchievementId { get; set; }

    [ForeignKey(nameof(AchievementId))]
    public Achievement? Achievement { get; set; }

    [Required]
    public DateTime UnlockedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
