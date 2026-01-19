// <copyright file="SessionAchievementRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class SessionAchievementRepository : ISessionAchievementRepository
{
    private readonly ApplicationDbContext context;

    public SessionAchievementRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<SessionAchievement?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<SessionAchievement>()
            .Include(sa => sa.Session)
            .Include(sa => sa.Achievement)
            .FirstOrDefaultAsync(sa => sa.Id == id);
    }

    public async Task<IEnumerable<SessionAchievement>> GetAllAsync()
    {
        return await this.context.Set<SessionAchievement>()
            .Include(sa => sa.Session)
            .Include(sa => sa.Achievement)
            .ToListAsync();
    }

    public async Task<SessionAchievement> CreateAsync(SessionAchievement sessionAchievement)
    {
        this.context.Set<SessionAchievement>().Add(sessionAchievement);
        await this.context.SaveChangesAsync();
        return sessionAchievement;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sessionAchievement = await this.context.Set<SessionAchievement>().FindAsync(id);
        if (sessionAchievement == null)
        {
            return false;
        }

        this.context.Set<SessionAchievement>().Remove(sessionAchievement);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SessionAchievement>> GetBySessionIdAsync(Guid sessionId)
    {
        return await this.context.Set<SessionAchievement>()
            .Include(sa => sa.Session)
            .Include(sa => sa.Achievement)
            .Where(sa => sa.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionAchievement>> GetByAchievementIdAsync(Guid achievementId)
    {
        return await this.context.Set<SessionAchievement>()
            .Include(sa => sa.Session)
            .Include(sa => sa.Achievement)
            .Where(sa => sa.AchievementId == achievementId)
            .ToListAsync();
    }

    public async Task<SessionAchievement?> GetBySessionAndAchievementIdAsync(Guid sessionId, Guid achievementId)
    {
        return await this.context.Set<SessionAchievement>()
            .Include(sa => sa.Session)
            .Include(sa => sa.Achievement)
            .FirstOrDefaultAsync(sa => sa.SessionId == sessionId && sa.AchievementId == achievementId);
    }
}
