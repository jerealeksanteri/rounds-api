// "// Copyright (c) 2026 Jere Niemi. All rights reserved."

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class FriendGroupRepository : IFriendGroupRepository
{
    private readonly ApplicationDbContext _context;

    public FriendGroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FriendGroup?> GetByIdAsync(Guid id)
    {
        return await _context.FriendGroups
            .Include(g => g.Owner)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<FriendGroup>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.FriendGroups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .Where(g => g.OwnerId == ownerId)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public Task<IEnumerable<FriendGroup>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<FriendGroup> CreateAsync(FriendGroup friendGroup)
    {
        _context.FriendGroups.Add(friendGroup);
        await _context.SaveChangesAsync();
        return friendGroup;
    }

    public async Task<FriendGroup> UpdateAsync(FriendGroup friendGroup)
    {
        _context.FriendGroups.Update(friendGroup);
        await _context.SaveChangesAsync();
        return friendGroup;
    }

    public async Task<bool> DeleteAsync(FriendGroup friendGroup)
    {
        _context.FriendGroups.Remove(friendGroup);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<FriendGroup>> GetByFriendIdAsync(Guid userId)
    {
        return await _context.FriendGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .OrderBy(g => g.Name)
            .ToListAsync();
    }
}
