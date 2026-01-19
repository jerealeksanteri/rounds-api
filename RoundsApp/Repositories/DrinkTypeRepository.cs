// <copyright file="DrinkTypeRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class DrinkTypeRepository : IDrinkTypeRepository
{
    private readonly ApplicationDbContext context;

    public DrinkTypeRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<DrinkType?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<DrinkType>()
            .Include(dt => dt.Drinks)
            .FirstOrDefaultAsync(dt => dt.Id == id);
    }

    public async Task<IEnumerable<DrinkType>> GetAllAsync()
    {
        return await this.context.Set<DrinkType>()
            .Include(dt => dt.Drinks)
            .ToListAsync();
    }

    public async Task<DrinkType> CreateAsync(DrinkType drinkType)
    {
        this.context.Set<DrinkType>().Add(drinkType);
        await this.context.SaveChangesAsync();
        return drinkType;
    }

    public async Task<DrinkType> UpdateAsync(DrinkType drinkType)
    {
        this.context.Set<DrinkType>().Update(drinkType);
        await this.context.SaveChangesAsync();
        return drinkType;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var drinkType = await this.context.Set<DrinkType>().FindAsync(id);
        if (drinkType == null)
        {
            return false;
        }

        this.context.Set<DrinkType>().Remove(drinkType);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<DrinkType?> GetByNameAsync(string name)
    {
        return await this.context.Set<DrinkType>()
            .Include(dt => dt.Drinks)
            .FirstOrDefaultAsync(dt => dt.Name == name);
    }
}
