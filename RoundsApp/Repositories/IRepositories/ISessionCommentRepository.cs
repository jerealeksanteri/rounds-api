// <copyright file="ISessionCommentRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ISessionCommentRepository
{
    Task<SessionComment?> GetByIdAsync(Guid id);

    Task<IEnumerable<SessionComment>> GetAllAsync();

    Task<SessionComment> CreateAsync(SessionComment comment);

    Task<SessionComment> UpdateAsync(SessionComment comment);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<SessionComment>> GetBySessionIdAsync(Guid sessionId);

    Task<IEnumerable<SessionComment>> GetByUserIdAsync(Guid userId);
}
