// <copyright file="IDrinkRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IDrinkRepository
{
    Task<Drink?> GetByIdAsync(Guid id);

    Task<IEnumerable<Drink>> GetAllAsync();

    Task<Drink> CreateAsync(Drink drink);

    Task<Drink> UpdateAsync(Drink drink);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<Drink>> GetByDrinkTypeIdAsync(Guid drinkTypeId);

    Task<IEnumerable<Drink>> SearchByNameAsync(string name);
}
