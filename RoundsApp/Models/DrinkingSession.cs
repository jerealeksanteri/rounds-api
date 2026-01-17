// <copyright file="DrinkingSession.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RoundsApp.Models;

/// <summary>
/// Represents a drinking session.
/// </summary>
public class DrinkingSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public List<SessionParticipation> Participants { get; set; } = new List<SessionParticipation>();
    public List<DrinkingSessionImage> Images { get; set; } = new List<DrinkingSessionImage>();

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [ForeignKey("CreatedBy")]
    public Guid CreatedById { get; set; }

    public ApplicationUser? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UpdatedBy")]
    public Guid? UpdatedById { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }
}
