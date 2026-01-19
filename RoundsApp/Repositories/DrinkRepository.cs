// <copyright file="DrinkRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class DrinkRepository : IDrinkRepository
{
    private readonly ApplicationDbContext context;

    public DrinkRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<Drink?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<Drink>()
            .Include(d => d.DrinkType)
            .Include(d => d.Images)
            .Include(d => d.UserDrinks)
            .Include(d => d.FavouritedBy)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Drink>> GetAllAsync()
    {
        return await this.context.Set<Drink>()
            .Include(d => d.DrinkType)
            .Include(d => d.Images)
            .Include(d => d.UserDrinks)
            .Include(d => d.FavouritedBy)
            .ToListAsync();
    }

    public async Task<Drink> CreateAsync(Drink drink)
    {
        this.context.Set<Drink>().Add(drink);
        await this.context.SaveChangesAsync();
        return drink;
    }

    public async Task<Drink> UpdateAsync(Drink drink)
    {
        this.context.Set<Drink>().Update(drink);
        await this.context.SaveChangesAsync();
        return drink;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var drink = await this.context.Set<Drink>().FindAsync(id);
        if (drink == null)
        {
            return false;
        }

        this.context.Set<Drink>().Remove(drink);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Drink>> GetByDrinkTypeIdAsync(Guid drinkTypeId)
    {
        return await this.context.Set<Drink>()
            .Include(d => d.DrinkType)
            .Include(d => d.Images)
            .Include(d => d.UserDrinks)
            .Include(d => d.FavouritedBy)
            .Where(d => d.DrinkTypeId == drinkTypeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Drink>> SearchByNameAsync(string name)
    {
        return await this.context.Set<Drink>()
            .Include(d => d.DrinkType)
            .Include(d => d.Images)
            .Include(d => d.UserDrinks)
            .Include(d => d.FavouritedBy)
            .Where(d => d.Name.Contains(name))
            .ToListAsync();
    }
}
