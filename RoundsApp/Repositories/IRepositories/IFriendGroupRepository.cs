// "// Copyright (c) 2026 Jere Niemi. All rights reserved."

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IFriendGroupRepository
{
    Task<FriendGroup?> GetByIdAsync(Guid id);
    Task<IEnumerable<FriendGroup>> GetAllAsync();
    Task<FriendGroup> CreateAsync(FriendGroup friendGroup);
    Task<FriendGroup> UpdateAsync(FriendGroup friendGroup);
    Task<bool> DeleteAsync(FriendGroup friendGroup);
    Task<IEnumerable<FriendGroup>> GetByOwnerIdAsync(Guid id);
    Task<IEnumerable<FriendGroup>> GetByFriendIdAsync(Guid id);
}
