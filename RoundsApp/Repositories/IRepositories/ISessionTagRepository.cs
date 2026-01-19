// <copyright file="ISessionTagRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ISessionTagRepository
{
    Task<SessionTag?> GetByIdAsync(Guid id);

    Task<IEnumerable<SessionTag>> GetAllAsync();

    Task<SessionTag> CreateAsync(SessionTag tag);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<SessionTag>> GetBySessionIdAsync(Guid sessionId);

    Task<IEnumerable<SessionTag>> GetByTagNameAsync(string tagName);
}
