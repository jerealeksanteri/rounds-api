// <copyright file="FriendGroupValidationService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Services;

public class FriendGroupValidationService : IFriendGroupValidationService
{
    private readonly IFriendshipRepository _friendshipRepository;

    public FriendGroupValidationService(IFriendshipRepository friendshipRepository)
    {
        _friendshipRepository = friendshipRepository;
    }

    public async Task<bool> AreFriendsAsync(Guid userId, Guid friendId)
    {
        var friends = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
        return friends.Any(f =>
            (f.UserId == friendId || f.FriendId == friendId) &&
            f.Status == FriendshipStatus.Accepted);
    }

    public async Task<bool> AreAllFriendsAsync(Guid userId, IEnumerable<Guid> friendIds)
    {
        var friends = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
        var acceptedFriendIds = friends
            .Where(f => f.Status == FriendshipStatus.Accepted)
            .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
            .ToHashSet();

        return friendIds.All(id => acceptedFriendIds.Contains(id));
    }

    public async Task<IEnumerable<Guid>> FilterNonFriendsAsync(Guid userId, IEnumerable<Guid> userIds)
    {
        var friends = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
        var acceptedFriendIds = friends
            .Where(f => f.Status == FriendshipStatus.Accepted)
            .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
            .ToHashSet();

        return userIds.Where(id => !acceptedFriendIds.Contains(id));
    }
}
