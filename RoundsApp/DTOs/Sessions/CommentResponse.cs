// <copyright file="CommentResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Users;

namespace RoundsApp.DTOs.Sessions;

public class CommentResponse
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public UserResponse? User { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid CreatedById { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<CommentMentionResponse> Mentions { get; set; } = new();
}
