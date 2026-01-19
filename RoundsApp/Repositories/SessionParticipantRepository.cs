// <copyright file="SessionParticipantRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class SessionParticipantRepository : ISessionParticipantRepository
{
    private readonly ApplicationDbContext context;

    public SessionParticipantRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<SessionParticipant?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<SessionParticipant>()
            .Include(p => p.Session)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<SessionParticipant>> GetAllAsync()
    {
        return await this.context.Set<SessionParticipant>()
            .Include(p => p.Session)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<SessionParticipant> CreateAsync(SessionParticipant participant)
    {
        this.context.Set<SessionParticipant>().Add(participant);
        await this.context.SaveChangesAsync();
        return participant;
    }

    public async Task<SessionParticipant> UpdateAsync(SessionParticipant participant)
    {
        this.context.Set<SessionParticipant>().Update(participant);
        await this.context.SaveChangesAsync();
        return participant;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var participant = await this.context.Set<SessionParticipant>().FindAsync(id);
        if (participant == null)
        {
            return false;
        }

        this.context.Set<SessionParticipant>().Remove(participant);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SessionParticipant>> GetBySessionIdAsync(Guid sessionId)
    {
        return await this.context.Set<SessionParticipant>()
            .Include(p => p.Session)
            .Include(p => p.User)
            .Where(p => p.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionParticipant>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<SessionParticipant>()
            .Include(p => p.Session)
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<SessionParticipant?> GetBySessionAndUserAsync(Guid sessionId, Guid userId)
    {
        return await this.context.Set<SessionParticipant>()
            .Include(p => p.Session)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.UserId == userId);
    }
}
