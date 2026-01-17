// <copyright file="DrinkType.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RoundsApp.Models;

/// <summary>
/// Enum representing different types of drinks.
/// </summary>
public enum DrinkType
{
    Beer,
    LongDrink,
    Cider,
    Seltzer,
    Wine,
    Spirit,
    Cocktail,
    NonAlcoholic,
}
