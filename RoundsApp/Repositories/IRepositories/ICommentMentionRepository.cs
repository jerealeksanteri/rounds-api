// <copyright file="ICommentMentionRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ICommentMentionRepository
{
    Task<CommentMention?> GetByIdAsync(Guid id);

    Task<IEnumerable<CommentMention>> GetByCommentIdAsync(Guid commentId);

    Task<IEnumerable<CommentMention>> GetByUserIdAsync(Guid userId);

    Task<CommentMention> CreateAsync(CommentMention mention);

    Task CreateMultipleAsync(IEnumerable<CommentMention> mentions);

    Task<bool> DeleteByCommentIdAsync(Guid commentId);
}
