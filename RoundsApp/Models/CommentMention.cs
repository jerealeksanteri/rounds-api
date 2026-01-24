// <copyright file="CommentMention.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoundsApp.Models;

public class CommentMention
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CommentId { get; set; }

    [ForeignKey(nameof(CommentId))]
    public SessionComment? Comment { get; set; }

    [Required]
    public Guid MentionedUserId { get; set; }

    [ForeignKey(nameof(MentionedUserId))]
    public ApplicationUser? MentionedUser { get; set; }

    [Required]
    public int StartPosition { get; set; }

    [Required]
    public int Length { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
