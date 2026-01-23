// <copyright file="MentionService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Services;

public partial class MentionService : IMentionService
{
    private readonly ICommentMentionRepository _mentionRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public MentionService(
        ICommentMentionRepository mentionRepository,
        UserManager<ApplicationUser> userManager)
    {
        _mentionRepository = mentionRepository;
        _userManager = userManager;
    }

    public IEnumerable<MentionParseResult> ParseMentions(string content)
    {
        var matches = MentionPattern().Matches(content);
        return matches.Select(m => new MentionParseResult
        {
            Username = m.Groups[1].Value,
            StartPosition = m.Index,
            Length = m.Length,
        }).ToList();
    }

    public async Task<IEnumerable<CommentMention>> ParseAndCreateMentionsAsync(
        Guid commentId,
        string content,
        Guid createdById)
    {
        var parseResults = ParseMentions(content);
        var mentions = new List<CommentMention>();

        foreach (var result in parseResults)
        {
            var user = await _userManager.FindByNameAsync(result.Username);
            if (user != null)
            {
                mentions.Add(new CommentMention
                {
                    Id = Guid.NewGuid(),
                    CommentId = commentId,
                    MentionedUserId = user.Id,
                    StartPosition = result.StartPosition,
                    Length = result.Length,
                    CreatedAt = DateTime.UtcNow,
                });
            }
        }

        if (mentions.Count > 0)
        {
            await _mentionRepository.CreateMultipleAsync(mentions);
        }

        return mentions;
    }

    [GeneratedRegex(@"@(\w+)")]
    private static partial Regex MentionPattern();
}
