// <copyright file="NotificationRepository.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;

namespace RoundsApp.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext context;

    public NotificationRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await this.context.Set<Notification>()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await this.context.Set<Notification>()
            .Include(n => n.User)
            .ToListAsync();
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        this.context.Set<Notification>().Add(notification);
        await this.context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        this.context.Set<Notification>().Update(notification);
        await this.context.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var notification = await this.context.Set<Notification>().FindAsync(id);
        if (notification == null)
        {
            return false;
        }

        this.context.Set<Notification>().Remove(notification);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        return await this.context.Set<Notification>()
            .Include(n => n.User)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
    {
        return await this.context.Set<Notification>()
            .Include(n => n.User)
            .Where(n => n.UserId == userId && !n.Read)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(Guid id)
    {
        var notification = await this.context.Set<Notification>().FindAsync(id);
        if (notification == null)
        {
            return false;
        }

        notification.Read = true;
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await this.context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.Read)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.Read = true;
        }

        await this.context.SaveChangesAsync();
        return true;
    }
}
