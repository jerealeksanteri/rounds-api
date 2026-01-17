// <copyright file="DrinkingSessionImage.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RoundsApp.Models;

/// <summary>
/// Represents an image associated with a drinking session.
/// </summary>
public class DrinkingSessionImage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("DrinkingSession")]
    public Guid DrinkingSessionId { get; set; }
    public DrinkingSession? DrinkingSession { get; set; }

    [Required]
    public byte[] ? ImageData { get; set; }

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
