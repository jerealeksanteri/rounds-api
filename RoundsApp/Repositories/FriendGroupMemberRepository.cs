// "// Copyright (c) 2026 Jere Niemi. All rights reserved."

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class FriendGroupMemberRepository : IFriendGroupMemberRepository
{
    private readonly ApplicationDbContext _context;

    public FriendGroupMemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FriendGroupMember?> GetByIdAsync(Guid groupId, Guid userId)
    {
        return await _context.FriendGroupMembers
            .Include(ud => ud.Group)
            .Include(ud => ud.User)
            .Where(fgm => fgm.GroupId == groupId && fgm.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FriendGroupMember>> GetByGroupIdAsync(Guid groupId)
    {
        return await _context.FriendGroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.User!.UserName)
            .ToListAsync();
    }

    public async Task CreateMultipleAsync(IEnumerable<FriendGroupMember> members)
    {
        await _context.FriendGroupMembers.AddRangeAsync(members);
        await _context.SaveChangesAsync();
    }

    public async Task<FriendGroupMember> CreateAsync(FriendGroupMember friendGroupMember)
    {
        _context.FriendGroupMembers.Add(friendGroupMember);
        await _context.SaveChangesAsync();

        return friendGroupMember;
    }

    public async Task<bool> DeleteAsync(Guid groupId, Guid userId)
    {
        var friendGroupMember = await GetByIdAsync(groupId, userId);
        if (friendGroupMember == null)
        {
            return false;
        }

        _context.FriendGroupMembers.Remove(friendGroupMember);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsMemberAsync(Guid groupId, Guid userId)
    {
        var friendGroupMember = await GetByIdAsync(groupId, userId);
        if (friendGroupMember == null)
        {
            return false;
        }

        return true;
    }
}
