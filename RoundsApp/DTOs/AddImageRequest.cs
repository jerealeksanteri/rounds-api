// <copyright file="AddImageRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace RoundsApp.DTOs;

public class AddImageRequest
{
    [Required]
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
}
