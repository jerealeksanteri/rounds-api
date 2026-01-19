// <copyright file="ISessionParticipantRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ISessionParticipantRepository
{
    Task<SessionParticipant?> GetByIdAsync(Guid id);

    Task<IEnumerable<SessionParticipant>> GetAllAsync();

    Task<SessionParticipant> CreateAsync(SessionParticipant participant);

    Task<SessionParticipant> UpdateAsync(SessionParticipant participant);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<SessionParticipant>> GetBySessionIdAsync(Guid sessionId);

    Task<IEnumerable<SessionParticipant>> GetByUserIdAsync(Guid userId);

    Task<SessionParticipant?> GetBySessionAndUserAsync(Guid sessionId, Guid userId);
}
