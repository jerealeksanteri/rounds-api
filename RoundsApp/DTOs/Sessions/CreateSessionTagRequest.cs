// <copyright file="CreateSessionTagRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class CreateSessionTagRequest
{
    public Guid SessionId { get; set; }

    public string Tag { get; set; } = string.Empty;
}
