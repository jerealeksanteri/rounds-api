// <copyright file="ISessionAchievementRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ISessionAchievementRepository
{
    Task<SessionAchievement?> GetByIdAsync(Guid id);

    Task<IEnumerable<SessionAchievement>> GetAllAsync();

    Task<SessionAchievement> CreateAsync(SessionAchievement sessionAchievement);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<SessionAchievement>> GetBySessionIdAsync(Guid sessionId);

    Task<IEnumerable<SessionAchievement>> GetByAchievementIdAsync(Guid achievementId);

    Task<SessionAchievement?> GetBySessionAndAchievementIdAsync(Guid sessionId, Guid achievementId);
}
