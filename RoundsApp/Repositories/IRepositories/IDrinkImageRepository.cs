// <copyright file="IDrinkImageRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IDrinkImageRepository
{
    Task<DrinkImage?> GetByIdAsync(Guid id);

    Task<IEnumerable<DrinkImage>> GetAllAsync();

    Task<DrinkImage> CreateAsync(DrinkImage image);

    Task<DrinkImage> UpdateAsync(DrinkImage image);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<DrinkImage>> GetByDrinkIdAsync(Guid drinkId);
}
