// <copyright file="SessionInviteRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class SessionInviteRepository : ISessionInviteRepository
{
    private readonly ApplicationDbContext context;

    public SessionInviteRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<SessionInvite?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<SessionInvite>()
            .Include(i => i.Session)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<SessionInvite>> GetAllAsync()
    {
        return await this.context.Set<SessionInvite>()
            .Include(i => i.Session)
            .Include(i => i.User)
            .ToListAsync();
    }

    public async Task<SessionInvite> CreateAsync(SessionInvite invite)
    {
        this.context.Set<SessionInvite>().Add(invite);
        await this.context.SaveChangesAsync();
        return invite;
    }

    public async Task<SessionInvite> UpdateAsync(SessionInvite invite)
    {
        this.context.Set<SessionInvite>().Update(invite);
        await this.context.SaveChangesAsync();
        return invite;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var invite = await this.context.Set<SessionInvite>().FindAsync(id);
        if (invite == null)
        {
            return false;
        }

        this.context.Set<SessionInvite>().Remove(invite);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SessionInvite>> GetBySessionIdAsync(Guid sessionId)
    {
        return await this.context.Set<SessionInvite>()
            .Include(i => i.Session)
            .Include(i => i.User)
            .Where(i => i.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionInvite>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<SessionInvite>()
            .Include(i => i.Session)
            .Include(i => i.User)
            .Where(i => i.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionInvite>> GetPendingInvitesByUserIdAsync(Guid userId)
    {
        return await this.context.Set<SessionInvite>()
            .Include(i => i.Session)
            .Include(i => i.User)
            .Where(i => i.UserId == userId && i.Status == "pending")
            .ToListAsync();
    }
}
