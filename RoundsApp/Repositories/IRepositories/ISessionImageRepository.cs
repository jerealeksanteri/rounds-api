// <copyright file="ISessionImageRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ISessionImageRepository
{
    Task<SessionImage?> GetByIdAsync(Guid id);

    Task<IEnumerable<SessionImage>> GetAllAsync();

    Task<SessionImage> CreateAsync(SessionImage image);

    Task<SessionImage> UpdateAsync(SessionImage image);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<SessionImage>> GetBySessionIdAsync(Guid sessionId);
}
