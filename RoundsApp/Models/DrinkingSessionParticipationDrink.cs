// <copyright file="SessionParticipationDrink.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RoundsApp.Models;

/// <summary>
/// Represents a drink consumed by a participant in a drinking session.
/// </summary>
public class DrinkingSessionParticipationDrink
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("DrinkingSessionParticipation")]
    public Guid DrinkingSessionParticipationId { get; set; }
    public DrinkingSessionParticipation? DrinkingSessionParticipation { get; set; }

    [Required]
    [ForeignKey("Drink")]
    public Guid DrinkId { get; set; }
    public Drink? Drink { get; set; }

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
