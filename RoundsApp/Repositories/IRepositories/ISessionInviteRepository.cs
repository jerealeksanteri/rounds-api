// <copyright file="ISessionInviteRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ISessionInviteRepository
{
    Task<SessionInvite?> GetByIdAsync(Guid id);

    Task<IEnumerable<SessionInvite>> GetAllAsync();

    Task<SessionInvite> CreateAsync(SessionInvite invite);

    Task<SessionInvite> UpdateAsync(SessionInvite invite);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<SessionInvite>> GetBySessionIdAsync(Guid sessionId);

    Task<IEnumerable<SessionInvite>> GetByUserIdAsync(Guid userId);

    Task<IEnumerable<SessionInvite>> GetPendingInvitesByUserIdAsync(Guid userId);
}
