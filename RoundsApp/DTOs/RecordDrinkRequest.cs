// <copyright file="RecordDrinkRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace RoundsApp.DTOs;

public class RecordDrinkRequest
{
    [Required]
    public Guid ParticipantId { get; set; }

    [Required]
    public Guid DrinkId { get; set; }
}
