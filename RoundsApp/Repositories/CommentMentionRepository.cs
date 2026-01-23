// <copyright file="CommentMentionRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class CommentMentionRepository : ICommentMentionRepository
{
    private readonly ApplicationDbContext context;

    public CommentMentionRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<CommentMention?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<CommentMention>()
            .Include(m => m.MentionedUser)
            .Include(m => m.Comment)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<CommentMention>> GetByCommentIdAsync(Guid commentId)
    {
        return await this.context.Set<CommentMention>()
            .Include(m => m.MentionedUser)
            .Where(m => m.CommentId == commentId)
            .OrderBy(m => m.StartPosition)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommentMention>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<CommentMention>()
            .Include(m => m.Comment)
                .ThenInclude(c => c!.Session)
            .Include(m => m.Comment)
                .ThenInclude(c => c!.User)
            .Where(m => m.MentionedUserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<CommentMention> CreateAsync(CommentMention mention)
    {
        this.context.Set<CommentMention>().Add(mention);
        await this.context.SaveChangesAsync();
        return mention;
    }

    public async Task CreateMultipleAsync(IEnumerable<CommentMention> mentions)
    {
        await this.context.Set<CommentMention>().AddRangeAsync(mentions);
        await this.context.SaveChangesAsync();
    }

    public async Task<bool> DeleteByCommentIdAsync(Guid commentId)
    {
        var mentions = await this.context.Set<CommentMention>()
            .Where(m => m.CommentId == commentId)
            .ToListAsync();

        this.context.Set<CommentMention>().RemoveRange(mentions);
        await this.context.SaveChangesAsync();
        return true;
    }
}
