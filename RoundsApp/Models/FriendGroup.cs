// "// Copyright (c) 2026 Jere Niemi. All rights reserved."

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoundsApp.Models;

public class FriendGroup
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser? Owner { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid UpdatedById { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }

    public ICollection<FriendGroupMember> Members { get; set; } = new List<FriendGroupMember>();
}
