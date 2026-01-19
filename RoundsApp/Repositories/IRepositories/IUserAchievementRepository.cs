// <copyright file="IUserAchievementRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IUserAchievementRepository
{
    Task<UserAchievement?> GetByIdAsync(Guid id);

    Task<IEnumerable<UserAchievement>> GetAllAsync();

    Task<UserAchievement> CreateAsync(UserAchievement userAchievement);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<UserAchievement>> GetByUserIdAsync(Guid userId);

    Task<IEnumerable<UserAchievement>> GetByAchievementIdAsync(Guid achievementId);

    Task<UserAchievement?> GetByUserAndAchievementIdAsync(Guid userId, Guid achievementId);
}
