// Copyright (c) 2026 Jere Niemi. All rights reserved.

using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace RoundsApp.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Fetch user id and add to group if found
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Fetch user id and remove from group if found
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
