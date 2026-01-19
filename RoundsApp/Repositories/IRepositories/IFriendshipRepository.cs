// <copyright file="IFriendshipRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface IFriendshipRepository
{
    Task<Friendship?> GetByIdAsync(Guid userId, Guid friendId);

    Task<IEnumerable<Friendship>> GetAllAsync();

    Task<Friendship> CreateAsync(Friendship friendship);

    Task<Friendship> UpdateAsync(Friendship friendship);

    Task<bool> DeleteAsync(Guid userId, Guid friendId);

    Task<IEnumerable<Friendship>> GetByUserIdAsync(Guid userId);

    Task<IEnumerable<Friendship>> GetFriendsByUserIdAsync(Guid userId);

    Task<IEnumerable<Friendship>> GetPendingRequestsByUserIdAsync(Guid userId);

    Task<IEnumerable<Friendship>> GetSentRequestsByUserIdAsync(Guid userId);
}
