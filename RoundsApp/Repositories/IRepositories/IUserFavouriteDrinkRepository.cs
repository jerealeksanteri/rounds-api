// <copyright file="IUserFavouriteDrinkRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IUserFavouriteDrinkRepository
{
    Task<UserFavouriteDrink?> GetByIdAsync(Guid id);

    Task<IEnumerable<UserFavouriteDrink>> GetAllAsync();

    Task<UserFavouriteDrink> CreateAsync(UserFavouriteDrink favourite);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<UserFavouriteDrink>> GetByUserIdAsync(Guid userId);

    Task<UserFavouriteDrink?> GetByUserAndDrinkIdAsync(Guid userId, Guid drinkId);
}
