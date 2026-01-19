// <copyright file="IDrinkingSessionRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IDrinkingSessionRepository
{
    Task<DrinkingSession?> GetByIdAsync(Guid id);

    Task<IEnumerable<DrinkingSession>> GetAllAsync();

    Task<DrinkingSession> CreateAsync(DrinkingSession session);

    Task<DrinkingSession> UpdateAsync(DrinkingSession session);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<DrinkingSession>> GetByUserIdAsync(Guid userId);

    Task<IEnumerable<DrinkingSession>> GetUpcomingSessionsAsync();
}
