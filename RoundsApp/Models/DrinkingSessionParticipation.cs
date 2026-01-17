// <copyright file="SessionParticipation.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RoundsApp.Models;

/// <summary>
/// Represents a participant in a drinking session.
/// </summary>
public class DrinkingSessionParticipation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("DrinkingSession")]
    public Guid DrinkingSessionId { get; set; }

    public DrinkingSession? DrinkingSession { get; set; }

    [Required]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [Required]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public List<DrinkingSessionParticipationDrink> Drinks { get; set; } = new List<DrinkingSessionParticipationDrink>();

    [Required]
    [ForeignKey("CreatedBy")]
    public Guid CreatedById { get; set; }

    public ApplicationUser? CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UpdatedBy")]
    public Guid? UpdatedById { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
