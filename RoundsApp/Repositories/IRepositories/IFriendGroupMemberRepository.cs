// "// Copyright (c) 2026 Jere Niemi. All rights reserved."

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IFriendGroupMemberRepository
{
    Task<FriendGroupMember?> GetByIdAsync(Guid groupId, Guid userId);
    Task<IEnumerable<FriendGroupMember>> GetByGroupIdAsync(Guid groupId);
    Task<FriendGroupMember> CreateAsync(FriendGroupMember friendGroupMember);
    Task<bool> DeleteAsync(Guid groupId, Guid userId);
    Task CreateMultipleAsync(IEnumerable<FriendGroupMember> members);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId);
}
