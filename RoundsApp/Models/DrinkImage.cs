// <copyright file="DrinkImage.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoundsApp.Models;

public class DrinkImage
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DrinkId { get; set; }

    [ForeignKey(nameof(DrinkId))]
    public Drink? Drink { get; set; }

    [Required]
    public string Url { get; set; } = string.Empty;

    public string? Caption { get; set; }

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
