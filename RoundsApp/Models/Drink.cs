// <copyright file="Drink.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoundsApp.Models;

public class Drink
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid DrinkTypeId { get; set; }

    [ForeignKey(nameof(DrinkTypeId))]
    public DrinkType? DrinkType { get; set; }

    [Required]
    public decimal AlcoholContent { get; set; }

    [Required]
    public decimal VolumeLitres { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser? CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedById { get; set; }

    [ForeignKey(nameof(UpdatedById))]
    public ApplicationUser? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<DrinkImage> Images { get; set; } = new List<DrinkImage>();

    public ICollection<UserDrink> UserDrinks { get; set; } = new List<UserDrink>();

    public ICollection<UserFavouriteDrink> FavouritedBy { get; set; } = new List<UserFavouriteDrink>();
}
