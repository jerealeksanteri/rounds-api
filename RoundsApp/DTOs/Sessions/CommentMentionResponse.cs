// <copyright file="CommentMentionResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Users;

namespace RoundsApp.DTOs.Sessions;

public class CommentMentionResponse
{
    public Guid Id { get; set; }

    public Guid CommentId { get; set; }

    public Guid MentionedUserId { get; set; }

    public UserResponse? MentionedUser { get; set; }

    public int StartPosition { get; set; }

    public int Length { get; set; }
}
