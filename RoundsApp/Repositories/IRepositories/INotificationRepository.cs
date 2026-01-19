// <copyright file="INotificationRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Repositories.IRepositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);

    Task<IEnumerable<Notification>> GetAllAsync();

    Task<Notification> CreateAsync(Notification notification);

    Task<Notification> UpdateAsync(Notification notification);

    Task<bool> DeleteAsync(Guid id);

    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);

    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);

    Task<bool> MarkAsReadAsync(Guid id);

    Task<bool> MarkAllAsReadAsync(Guid userId);
}
