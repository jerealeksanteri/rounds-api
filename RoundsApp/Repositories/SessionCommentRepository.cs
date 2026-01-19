// <copyright file="SessionCommentRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class SessionCommentRepository : ISessionCommentRepository
{
    private readonly ApplicationDbContext context;

    public SessionCommentRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<SessionComment?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<SessionComment>()
            .Include(c => c.Session)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<SessionComment>> GetAllAsync()
    {
        return await this.context.Set<SessionComment>()
            .Include(c => c.Session)
            .Include(c => c.User)
            .ToListAsync();
    }

    public async Task<SessionComment> CreateAsync(SessionComment comment)
    {
        this.context.Set<SessionComment>().Add(comment);
        await this.context.SaveChangesAsync();
        return comment;
    }

    public async Task<SessionComment> UpdateAsync(SessionComment comment)
    {
        this.context.Set<SessionComment>().Update(comment);
        await this.context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var comment = await this.context.Set<SessionComment>().FindAsync(id);
        if (comment == null)
        {
            return false;
        }

        this.context.Set<SessionComment>().Remove(comment);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SessionComment>> GetBySessionIdAsync(Guid sessionId)
    {
        return await this.context.Set<SessionComment>()
            .Include(c => c.Session)
            .Include(c => c.User)
            .Where(c => c.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionComment>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<SessionComment>()
            .Include(c => c.Session)
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }
}
