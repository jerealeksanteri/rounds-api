# Feature: Real-Time Notifications with SignalR

## Overview
Add SignalR hub to push notifications in real-time when events occur (session invites, friend requests, mentions, achievements unlocked).

## Priority
**1st** - Foundation for other features (mentions, activity feed use this infrastructure)

## New Files

### Hub
**`Hubs/NotificationHub.cs`**
```csharp
namespace RoundsApp.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
```

### Service Interface
**`Services/INotificationService.cs`**
```csharp
namespace RoundsApp.Services;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, NotificationResponse notification);
    Task SendNotificationToMultipleAsync(IEnumerable<Guid> userIds, NotificationResponse notification);
    Task CreateAndSendAsync(Guid userId, string type, string title, string message, string? metadata = null);
}
```

### Service Implementation
**`Services/NotificationService.cs`**
```csharp
namespace RoundsApp.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        INotificationRepository notificationRepository)
    {
        _hubContext = hubContext;
        _notificationRepository = notificationRepository;
    }

    public async Task SendNotificationAsync(Guid userId, NotificationResponse notification)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationToMultipleAsync(IEnumerable<Guid> userIds, NotificationResponse notification)
    {
        var tasks = userIds.Select(userId => SendNotificationAsync(userId, notification));
        await Task.WhenAll(tasks);
    }

    public async Task CreateAndSendAsync(Guid userId, string type, string title, string message, string? metadata = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Metadata = metadata,
            Read = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _notificationRepository.CreateAsync(notification);

        var response = new NotificationResponse
        {
            Id = created.Id,
            UserId = created.UserId,
            Type = created.Type,
            Title = created.Title,
            Message = created.Message,
            Metadata = created.Metadata,
            Read = created.Read,
            CreatedAt = created.CreatedAt
        };

        await SendNotificationAsync(userId, response);
    }
}
```

## Modifications to Existing Files

### Program.cs
Add after `builder.Services.AddAuthorization()`:
```csharp
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();
```

Configure JWT for SignalR WebSocket connections:
```csharp
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
```

Map hub before `app.RunAsync()`:
```csharp
app.MapHub<NotificationHub>("/hubs/notifications");
```

### SessionInviteEndpoints.cs
In `CreateInvite`, after creating the invite:
```csharp
await notificationService.CreateAndSendAsync(
    invite.UserId,
    "session_invite",
    "New Session Invite",
    $"{currentUser.UserName} invited you to {session.Name}",
    JsonSerializer.Serialize(new { sessionId = invite.SessionId, inviteId = invite.Id })
);
```

### FriendshipEndpoints.cs
In `CreateFriendship`, after creating the friendship:
```csharp
await notificationService.CreateAndSendAsync(
    request.FriendId,
    "friend_request",
    "New Friend Request",
    $"{currentUser.UserName} sent you a friend request",
    JsonSerializer.Serialize(new { userId = currentUser.Id })
);
```

## Database Changes
None required - uses existing `Notification` model with JSONB metadata.

## Client Connection Example
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications", {
        accessTokenFactory: () => getAccessToken()
    })
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
});

await connection.start();
```

## Notification Types
| Type | Trigger | Metadata |
|------|---------|----------|
| `session_invite` | User invited to session | `{ sessionId, inviteId }` |
| `friend_request` | Friend request received | `{ userId }` |
| `mention` | @mentioned in comment | `{ commentId, sessionId }` |
| `achievement_unlocked` | Achievement earned | `{ achievementId, userAchievementId }` |

## Testing
1. Connect to SignalR hub with valid JWT
2. Create session invite targeting connected user
3. Verify `ReceiveNotification` event fires with correct payload
4. Verify notification persisted in database