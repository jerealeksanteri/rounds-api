// <copyright file="IDrinkTypeRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IDrinkTypeRepository
{
    Task<DrinkType?> GetByIdAsync(Guid id);

    Task<IEnumerable<DrinkType>> GetAllAsync();

    Task<DrinkType> CreateAsync(DrinkType drinkType);

    Task<DrinkType> UpdateAsync(DrinkType drinkType);

    Task<bool> DeleteAsync(Guid id);

    Task<DrinkType?> GetByNameAsync(string name);
}
