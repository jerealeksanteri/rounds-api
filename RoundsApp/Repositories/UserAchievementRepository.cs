// <copyright file="UserAchievementRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class UserAchievementRepository : IUserAchievementRepository
{
    private readonly ApplicationDbContext context;

    public UserAchievementRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<UserAchievement?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<UserAchievement>()
            .Include(ua => ua.User)
            .Include(ua => ua.Achievement)
            .FirstOrDefaultAsync(ua => ua.Id == id);
    }

    public async Task<IEnumerable<UserAchievement>> GetAllAsync()
    {
        return await this.context.Set<UserAchievement>()
            .Include(ua => ua.User)
            .Include(ua => ua.Achievement)
            .ToListAsync();
    }

    public async Task<UserAchievement> CreateAsync(UserAchievement userAchievement)
    {
        this.context.Set<UserAchievement>().Add(userAchievement);
        await this.context.SaveChangesAsync();
        return userAchievement;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userAchievement = await this.context.Set<UserAchievement>().FindAsync(id);
        if (userAchievement == null)
        {
            return false;
        }

        this.context.Set<UserAchievement>().Remove(userAchievement);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserAchievement>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<UserAchievement>()
            .Include(ua => ua.User)
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserAchievement>> GetByAchievementIdAsync(Guid achievementId)
    {
        return await this.context.Set<UserAchievement>()
            .Include(ua => ua.User)
            .Include(ua => ua.Achievement)
            .Where(ua => ua.AchievementId == achievementId)
            .ToListAsync();
    }

    public async Task<UserAchievement?> GetByUserAndAchievementIdAsync(Guid userId, Guid achievementId)
    {
        return await this.context.Set<UserAchievement>()
            .Include(ua => ua.User)
            .Include(ua => ua.Achievement)
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
    }
}
