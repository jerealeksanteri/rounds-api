// <copyright file="ISessionLocationRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface ISessionLocationRepository
{
    Task<SessionLocation?> GetByIdAsync(Guid id);

    Task<IEnumerable<SessionLocation>> GetAllAsync();

    Task<SessionLocation> CreateAsync(SessionLocation location);

    Task<SessionLocation> UpdateAsync(SessionLocation location);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<SessionLocation>> SearchByNameAsync(string name);
}
