// <copyright file="UserDrinkRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class UserDrinkRepository : IUserDrinkRepository
{
    private readonly ApplicationDbContext context;

    public UserDrinkRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<UserDrink?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<UserDrink>()
            .Include(ud => ud.User)
            .Include(ud => ud.Session)
            .Include(ud => ud.Drink)
            .FirstOrDefaultAsync(ud => ud.Id == id);
    }

    public async Task<IEnumerable<UserDrink>> GetAllAsync()
    {
        return await this.context.Set<UserDrink>()
            .Include(ud => ud.User)
            .Include(ud => ud.Session)
            .Include(ud => ud.Drink)
            .ToListAsync();
    }

    public async Task<UserDrink> CreateAsync(UserDrink userDrink)
    {
        this.context.Set<UserDrink>().Add(userDrink);
        await this.context.SaveChangesAsync();
        return userDrink;
    }

    public async Task<UserDrink> UpdateAsync(UserDrink userDrink)
    {
        this.context.Set<UserDrink>().Update(userDrink);
        await this.context.SaveChangesAsync();
        return userDrink;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userDrink = await this.context.Set<UserDrink>().FindAsync(id);
        if (userDrink == null)
        {
            return false;
        }

        this.context.Set<UserDrink>().Remove(userDrink);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserDrink>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<UserDrink>()
            .Include(ud => ud.User)
            .Include(ud => ud.Session)
            .Include(ud => ud.Drink)
            .Where(ud => ud.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDrink>> GetBySessionIdAsync(Guid sessionId)
    {
        return await this.context.Set<UserDrink>()
            .Include(ud => ud.User)
            .Include(ud => ud.Session)
            .Include(ud => ud.Drink)
            .Where(ud => ud.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDrink>> GetByUserAndSessionIdAsync(Guid userId, Guid sessionId)
    {
        return await this.context.Set<UserDrink>()
            .Include(ud => ud.User)
            .Include(ud => ud.Session)
            .Include(ud => ud.Drink)
            .Where(ud => ud.UserId == userId && ud.SessionId == sessionId)
            .ToListAsync();
    }
}
