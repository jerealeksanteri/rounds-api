// <copyright file="IAchievementRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IAchievementRepository
{
    Task<Achievement?> GetByIdAsync(Guid id);

    Task<IEnumerable<Achievement>> GetAllAsync();

    Task<Achievement> CreateAsync(Achievement achievement);

    Task<Achievement> UpdateAsync(Achievement achievement);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<Achievement>> GetByTypeAsync(string type);
}
