// <copyright file="FriendshipRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly ApplicationDbContext context;

    public FriendshipRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<Friendship?> GetByIdAsync(Guid userId, Guid friendId)
    {
        return await this.context.Set<Friendship>()
            .Include(f => f.User)
            .Include(f => f.Friend)
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId);
    }

    public async Task<IEnumerable<Friendship>> GetAllAsync()
    {
        return await this.context.Set<Friendship>()
            .Include(f => f.User)
            .Include(f => f.Friend)
            .ToListAsync();
    }

    public async Task<Friendship> CreateAsync(Friendship friendship)
    {
        this.context.Set<Friendship>().Add(friendship);
        await this.context.SaveChangesAsync();
        return friendship;
    }

    public async Task<Friendship> UpdateAsync(Friendship friendship)
    {
        this.context.Set<Friendship>().Update(friendship);
        await this.context.SaveChangesAsync();
        return friendship;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid friendId)
    {
        var friendship = await this.context.Set<Friendship>()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId);

        if (friendship == null)
        {
            return false;
        }

        this.context.Set<Friendship>().Remove(friendship);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Friendship>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<Friendship>()
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => f.UserId == userId || f.FriendId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Friendship>> GetFriendsByUserIdAsync(Guid userId)
    {
        return await this.context.Set<Friendship>()
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == "accepted")
            .ToListAsync();
    }

    public async Task<IEnumerable<Friendship>> GetPendingRequestsByUserIdAsync(Guid userId)
    {
        return await this.context.Set<Friendship>()
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => f.FriendId == userId && f.Status == "pending")
            .ToListAsync();
    }

    public async Task<IEnumerable<Friendship>> GetSentRequestsByUserIdAsync(Guid userId)
    {
        return await this.context.Set<Friendship>()
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => f.UserId == userId && f.Status == "pending")
            .ToListAsync();
    }
}
