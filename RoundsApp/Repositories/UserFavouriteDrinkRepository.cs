// <copyright file="UserFavouriteDrinkRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class UserFavouriteDrinkRepository : IUserFavouriteDrinkRepository
{
    private readonly ApplicationDbContext context;

    public UserFavouriteDrinkRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<UserFavouriteDrink?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<UserFavouriteDrink>()
            .Include(ufd => ufd.User)
            .Include(ufd => ufd.Drink)
            .FirstOrDefaultAsync(ufd => ufd.Id == id);
    }

    public async Task<IEnumerable<UserFavouriteDrink>> GetAllAsync()
    {
        return await this.context.Set<UserFavouriteDrink>()
            .Include(ufd => ufd.User)
            .Include(ufd => ufd.Drink)
            .ToListAsync();
    }

    public async Task<UserFavouriteDrink> CreateAsync(UserFavouriteDrink favourite)
    {
        this.context.Set<UserFavouriteDrink>().Add(favourite);
        await this.context.SaveChangesAsync();
        return favourite;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var favourite = await this.context.Set<UserFavouriteDrink>().FindAsync(id);
        if (favourite == null)
        {
            return false;
        }

        this.context.Set<UserFavouriteDrink>().Remove(favourite);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserFavouriteDrink>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<UserFavouriteDrink>()
            .Include(ufd => ufd.User)
            .Include(ufd => ufd.Drink)
            .Where(ufd => ufd.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserFavouriteDrink?> GetByUserAndDrinkIdAsync(Guid userId, Guid drinkId)
    {
        return await this.context.Set<UserFavouriteDrink>()
            .Include(ufd => ufd.User)
            .Include(ufd => ufd.Drink)
            .FirstOrDefaultAsync(ufd => ufd.UserId == userId && ufd.DrinkId == drinkId);
    }
}
