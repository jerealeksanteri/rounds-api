// <copyright file="AchievementRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class AchievementRepository : IAchievementRepository
{
    private readonly ApplicationDbContext context;

    public AchievementRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<Achievement?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<Achievement>()
            .Include(a => a.UserAchievements)
            .Include(a => a.SessionAchievements)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Achievement>> GetAllAsync()
    {
        return await this.context.Set<Achievement>()
            .Include(a => a.UserAchievements)
            .Include(a => a.SessionAchievements)
            .ToListAsync();
    }

    public async Task<Achievement> CreateAsync(Achievement achievement)
    {
        this.context.Set<Achievement>().Add(achievement);
        await this.context.SaveChangesAsync();
        return achievement;
    }

    public async Task<Achievement> UpdateAsync(Achievement achievement)
    {
        this.context.Set<Achievement>().Update(achievement);
        await this.context.SaveChangesAsync();
        return achievement;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var achievement = await this.context.Set<Achievement>().FindAsync(id);
        if (achievement == null)
        {
            return false;
        }

        this.context.Set<Achievement>().Remove(achievement);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Achievement>> GetByTypeAsync(string type)
    {
        return await this.context.Set<Achievement>()
            .Include(a => a.UserAchievements)
            .Include(a => a.SessionAchievements)
            .Where(a => a.Type == type)
            .ToListAsync();
    }
}
