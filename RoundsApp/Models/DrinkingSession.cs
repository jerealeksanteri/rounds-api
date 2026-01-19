// <copyright file="DrinkingSession.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoundsApp.Models;

public class DrinkingSession
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public Guid? LocationId { get; set; }

    [ForeignKey(nameof(LocationId))]
    public SessionLocation? Location { get; set; }

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

    public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();

    public ICollection<SessionInvite> Invites { get; set; } = new List<SessionInvite>();

    public ICollection<SessionComment> Comments { get; set; } = new List<SessionComment>();

    public ICollection<SessionImage> Images { get; set; } = new List<SessionImage>();

    public ICollection<SessionTag> Tags { get; set; } = new List<SessionTag>();

    public ICollection<SessionAchievement> Achievements { get; set; } = new List<SessionAchievement>();

    public ICollection<UserDrink> Drinks { get; set; } = new List<UserDrink>();
}
