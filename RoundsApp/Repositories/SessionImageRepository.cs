// <copyright file="SessionImageRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class SessionImageRepository : ISessionImageRepository
{
    private readonly ApplicationDbContext context;

    public SessionImageRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<SessionImage?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<SessionImage>()
            .Include(i => i.Session)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<SessionImage>> GetAllAsync()
    {
        return await this.context.Set<SessionImage>()
            .Include(i => i.Session)
            .ToListAsync();
    }

    public async Task<SessionImage> CreateAsync(SessionImage image)
    {
        this.context.Set<SessionImage>().Add(image);
        await this.context.SaveChangesAsync();
        return image;
    }

    public async Task<SessionImage> UpdateAsync(SessionImage image)
    {
        this.context.Set<SessionImage>().Update(image);
        await this.context.SaveChangesAsync();
        return image;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var image = await this.context.Set<SessionImage>().FindAsync(id);
        if (image == null)
        {
            return false;
        }

        this.context.Set<SessionImage>().Remove(image);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SessionImage>> GetBySessionIdAsync(Guid sessionId)
    {
        return await this.context.Set<SessionImage>()
            .Include(i => i.Session)
            .Where(i => i.SessionId == sessionId)
            .ToListAsync();
    }
}
