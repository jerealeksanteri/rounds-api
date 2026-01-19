// <copyright file="CreateCommentRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class CreateCommentRequest
{
    public Guid SessionId { get; set; }

    public string Content { get; set; } = string.Empty;
}
