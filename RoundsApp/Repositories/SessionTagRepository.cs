// <copyright file="SessionTagRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class SessionTagRepository : ISessionTagRepository
{
    private readonly ApplicationDbContext context;

    public SessionTagRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<SessionTag?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<SessionTag>()
            .Include(t => t.Session)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<SessionTag>> GetAllAsync()
    {
        return await this.context.Set<SessionTag>()
            .Include(t => t.Session)
            .ToListAsync();
    }

    public async Task<SessionTag> CreateAsync(SessionTag tag)
    {
        this.context.Set<SessionTag>().Add(tag);
        await this.context.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tag = await this.context.Set<SessionTag>().FindAsync(id);
        if (tag == null)
        {
            return false;
        }

        this.context.Set<SessionTag>().Remove(tag);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SessionTag>> GetBySessionIdAsync(Guid sessionId)
    {
        return await this.context.Set<SessionTag>()
            .Include(t => t.Session)
            .Where(t => t.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionTag>> GetByTagNameAsync(string tagName)
    {
        return await this.context.Set<SessionTag>()
            .Include(t => t.Session)
            .Where(t => t.Tag == tagName)
            .ToListAsync();
    }
}
