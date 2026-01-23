// "// Copyright (c) 2026 Jere Niemi. All rights reserved."

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RoundsApp.Models;

[PrimaryKey(nameof(GroupId), nameof(UserId))]
public class FriendGroupMember
{
    [Required]
    public Guid GroupId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public FriendGroup? Group { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid AddedById { get; set; }

    [ForeignKey(nameof(AddedById))]
    public ApplicationUser? AddedBy { get; set; }
}
