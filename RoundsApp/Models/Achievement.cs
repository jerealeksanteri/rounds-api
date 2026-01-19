// <copyright file="Achievement.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoundsApp.Models;

public class Achievement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    public string? Icon { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string Criteria { get; set; } = "{}";

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }

    [ForeignKey(nameof(UpdatedById))]
    public ApplicationUser? UpdatedBy { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    public ICollection<SessionAchievement> SessionAchievements { get; set; } = new List<SessionAchievement>();
}
