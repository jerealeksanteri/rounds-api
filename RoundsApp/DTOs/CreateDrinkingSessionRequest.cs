/// <copyright file="CreateDrinkingSessionRequest.cs" company="RoundsApp">
/// Copyright (c) RoundsApp. All rights reserved.
/// </copyright>

using System.ComponentModel.DataAnnotations;
using RoundsApp.Models;

namespace RoundsApp.DTOs;

public class CreateDrinkingSessionRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? ScheduledAt { get; set; }
}
