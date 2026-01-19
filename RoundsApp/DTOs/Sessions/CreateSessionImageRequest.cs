// <copyright file="CreateSessionImageRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class CreateSessionImageRequest
{
    public Guid SessionId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? Caption { get; set; }
}
