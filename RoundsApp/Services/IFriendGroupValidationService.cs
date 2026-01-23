// <copyright file="IFriendGroupValidationService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.Services;

public interface IFriendGroupValidationService
{
    Task<bool> AreFriendsAsync(Guid userId, Guid friendId);
    Task<bool> AreAllFriendsAsync(Guid userId, IEnumerable<Guid> friendIds);
    Task<IEnumerable<Guid>> FilterNonFriendsAsync(Guid userId, IEnumerable<Guid> userIds);
}
