// <copyright file="SessionLocationRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class SessionLocationRepository : ISessionLocationRepository
{
    private readonly ApplicationDbContext context;

    public SessionLocationRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<SessionLocation?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<SessionLocation>()
            .Include(l => l.Sessions)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<SessionLocation>> GetAllAsync()
    {
        return await this.context.Set<SessionLocation>()
            .Include(l => l.Sessions)
            .ToListAsync();
    }

    public async Task<SessionLocation> CreateAsync(SessionLocation location)
    {
        this.context.Set<SessionLocation>().Add(location);
        await this.context.SaveChangesAsync();
        return location;
    }

    public async Task<SessionLocation> UpdateAsync(SessionLocation location)
    {
        this.context.Set<SessionLocation>().Update(location);
        await this.context.SaveChangesAsync();
        return location;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var location = await this.context.Set<SessionLocation>().FindAsync(id);
        if (location == null)
        {
            return false;
        }

        this.context.Set<SessionLocation>().Remove(location);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SessionLocation>> SearchByNameAsync(string name)
    {
        return await this.context.Set<SessionLocation>()
            .Include(l => l.Sessions)
            .Where(l => l.Name.Contains(name))
            .ToListAsync();
    }
}
