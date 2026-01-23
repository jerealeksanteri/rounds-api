// <copyright file="IMentionService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Services;

public interface IMentionService
{
    Task<IEnumerable<CommentMention>> ParseAndCreateMentionsAsync(
        Guid commentId,
        string content,
        Guid createdById);

    IEnumerable<MentionParseResult> ParseMentions(string content);
}

public class MentionParseResult
{
    public string Username { get; set; } = string.Empty;

    public int StartPosition { get; set; }

    public int Length { get; set; }
}
