// <copyright file="IUserDrinkRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IUserDrinkRepository
{
    Task<UserDrink?> GetByIdAsync(Guid id);

    Task<IEnumerable<UserDrink>> GetAllAsync();

    Task<UserDrink> CreateAsync(UserDrink userDrink);

    Task<UserDrink> UpdateAsync(UserDrink userDrink);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<UserDrink>> GetByUserIdAsync(Guid userId);

    Task<IEnumerable<UserDrink>> GetBySessionIdAsync(Guid sessionId);

    Task<IEnumerable<UserDrink>> GetByUserAndSessionIdAsync(Guid userId, Guid sessionId);
}
