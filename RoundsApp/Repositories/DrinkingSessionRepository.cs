// <copyright file="DrinkingSessionRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class DrinkingSessionRepository : IDrinkingSessionRepository
{
    private readonly ApplicationDbContext context;

    public DrinkingSessionRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<DrinkingSession?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<DrinkingSession>()
            .Include(s => s.Location)
            .Include(s => s.Participants)
            .Include(s => s.Invites)
            .Include(s => s.Comments)
            .Include(s => s.Images)
            .Include(s => s.Tags)
            .Include(s => s.Achievements)
            .Include(s => s.Drinks)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<DrinkingSession>> GetAllAsync()
    {
        return await this.context.Set<DrinkingSession>()
            .Include(s => s.Location)
            .Include(s => s.Participants)
            .Include(s => s.Invites)
            .Include(s => s.Comments)
            .Include(s => s.Images)
            .Include(s => s.Tags)
            .Include(s => s.Achievements)
            .Include(s => s.Drinks)
            .ToListAsync();
    }

    public async Task<DrinkingSession> CreateAsync(DrinkingSession session)
    {
        this.context.Set<DrinkingSession>().Add(session);
        await this.context.SaveChangesAsync();
        return session;
    }

    public async Task<DrinkingSession> UpdateAsync(DrinkingSession session)
    {
        this.context.Set<DrinkingSession>().Update(session);
        await this.context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var session = await this.context.Set<DrinkingSession>().FindAsync(id);
        if (session == null)
        {
            return false;
        }

        this.context.Set<DrinkingSession>().Remove(session);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DrinkingSession>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<DrinkingSession>()
            .Include(s => s.Location)
            .Include(s => s.Participants)
            .Include(s => s.Invites)
            .Include(s => s.Comments)
            .Include(s => s.Images)
            .Include(s => s.Tags)
            .Include(s => s.Achievements)
            .Include(s => s.Drinks)
            .Where(s => s.CreatedById == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DrinkingSession>> GetUpcomingSessionsAsync()
    {
        return await this.context.Set<DrinkingSession>()
            .Include(s => s.Location)
            .Include(s => s.Participants)
            .Include(s => s.Invites)
            .Include(s => s.Comments)
            .Include(s => s.Images)
            .Include(s => s.Tags)
            .Include(s => s.Achievements)
            .Include(s => s.Drinks)
            .Where(s => s.StartsAt != null && s.StartsAt > DateTime.UtcNow)
            .OrderBy(s => s.StartsAt)
            .ToListAsync();
    }
}
