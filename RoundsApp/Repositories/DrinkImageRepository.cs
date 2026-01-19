// <copyright file="DrinkImageRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class DrinkImageRepository : IDrinkImageRepository
{
    private readonly ApplicationDbContext context;

    public DrinkImageRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<DrinkImage?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<DrinkImage>()
            .Include(i => i.Drink)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<DrinkImage>> GetAllAsync()
    {
        return await this.context.Set<DrinkImage>()
            .Include(i => i.Drink)
            .ToListAsync();
    }

    public async Task<DrinkImage> CreateAsync(DrinkImage image)
    {
        this.context.Set<DrinkImage>().Add(image);
        await this.context.SaveChangesAsync();
        return image;
    }

    public async Task<DrinkImage> UpdateAsync(DrinkImage image)
    {
        this.context.Set<DrinkImage>().Update(image);
        await this.context.SaveChangesAsync();
        return image;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var image = await this.context.Set<DrinkImage>().FindAsync(id);
        if (image == null)
        {
            return false;
        }

        this.context.Set<DrinkImage>().Remove(image);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DrinkImage>> GetByDrinkIdAsync(Guid drinkId)
    {
        return await this.context.Set<DrinkImage>()
            .Include(i => i.Drink)
            .Where(i => i.DrinkId == drinkId)
            .ToListAsync();
    }
}
