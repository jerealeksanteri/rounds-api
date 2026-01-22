// <copyright file="Friendship.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RoundsApp.Models;

[PrimaryKey(nameof(UserId), nameof(FriendId))]
public class Friendship
{
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    public Guid FriendId { get; set; }

    [ForeignKey(nameof(FriendId))]
    public ApplicationUser? Friend { get; set; }

    [Required]
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

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
}

public enum FriendshipStatus
{
    /// <summary>
    /// Pending status
    /// </summary>
    Pending,

    /// <summary>
    /// Accepted friend
    /// </summary>
    Accepted,

    /// <summary>
    /// Rejected friend
    /// </summary>
    Rejected,
}
