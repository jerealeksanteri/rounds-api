# Feature: Activity Feed

## Overview
Track user activities (sessions created/joined, achievements unlocked) and display friends' recent activities. Only shows activities from accepted friends with privacy controls.

## Priority
**4th** - Uses notification patterns and friendship infrastructure

## Activity Types (User Specified)
- `session_created` - User created a new drinking session
- `session_joined` - User joined a session as participant
- `achievement_unlocked` - User unlocked an achievement

## New Files

### Model

**`Models/ActivityFeedItem.cs`**
```csharp
namespace RoundsApp.Models;

public class ActivityFeedItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActivityType { get; set; } = string.Empty;

    [Required]
    public Guid RelatedEntityId { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Required]
    public bool IsPublic { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Repository Interface

**`Repositories/IRepositories/IActivityFeedRepository.cs`**
```csharp
namespace RoundsApp.Repositories.IRepositories;

public interface IActivityFeedRepository
{
    Task<ActivityFeedItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<ActivityFeedItem>> GetAllAsync();
    Task<ActivityFeedItem> CreateAsync(ActivityFeedItem item);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ActivityFeedItem>> GetByUserIdAsync(Guid userId, int limit = 50, int offset = 0);
    Task<IEnumerable<ActivityFeedItem>> GetFriendsFeedAsync(Guid userId, int limit = 50, int offset = 0);
    Task<IEnumerable<ActivityFeedItem>> GetPublicFeedByUserIdAsync(Guid userId, int limit = 50, int offset = 0);
}
```

### Repository Implementation

**`Repositories/ActivityFeedRepository.cs`**
```csharp
namespace RoundsApp.Repositories;

public class ActivityFeedRepository : IActivityFeedRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IFriendshipRepository _friendshipRepository;

    public ActivityFeedRepository(
        ApplicationDbContext context,
        IFriendshipRepository friendshipRepository)
    {
        _context = context;
        _friendshipRepository = friendshipRepository;
    }

    public async Task<IEnumerable<ActivityFeedItem>> GetByUserIdAsync(
        Guid userId, int limit = 50, int offset = 0)
    {
        return await _context.ActivityFeedItems
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<ActivityFeedItem>> GetFriendsFeedAsync(
        Guid userId, int limit = 50, int offset = 0)
    {
        // Get accepted friend IDs
        var friendships = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
        var friendIds = friendships
            .Where(f => f.Status == "accepted")
            .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
            .ToHashSet();

        return await _context.ActivityFeedItems
            .Include(a => a.User)
            .Where(a => friendIds.Contains(a.UserId) && a.IsPublic)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<ActivityFeedItem>> GetPublicFeedByUserIdAsync(
        Guid userId, int limit = 50, int offset = 0)
    {
        return await _context.ActivityFeedItems
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.IsPublic)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<ActivityFeedItem> CreateAsync(ActivityFeedItem item)
    {
        _context.ActivityFeedItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await _context.ActivityFeedItems.FindAsync(id);
        if (item == null) return false;

        _context.ActivityFeedItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    // ... other methods
}
```

### Activity Feed Service Interface

**`Services/IActivityFeedService.cs`**
```csharp
namespace RoundsApp.Services;

public interface IActivityFeedService
{
    Task RecordSessionCreatedAsync(Guid userId, Guid sessionId, string sessionName, bool isPublic = true);
    Task RecordSessionJoinedAsync(Guid userId, Guid sessionId, string sessionName, bool isPublic = true);
    Task RecordAchievementUnlockedAsync(Guid userId, Guid userAchievementId, string achievementName, bool isPublic = true);
}
```

### Activity Feed Service Implementation

**`Services/ActivityFeedService.cs`**
```csharp
namespace RoundsApp.Services;

public class ActivityFeedService : IActivityFeedService
{
    private readonly IActivityFeedRepository _activityFeedRepository;

    public ActivityFeedService(IActivityFeedRepository activityFeedRepository)
    {
        _activityFeedRepository = activityFeedRepository;
    }

    public async Task RecordSessionCreatedAsync(
        Guid userId, Guid sessionId, string sessionName, bool isPublic = true)
    {
        var item = new ActivityFeedItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActivityType = "session_created",
            RelatedEntityId = sessionId,
            Metadata = JsonSerializer.Serialize(new { sessionName }),
            IsPublic = isPublic,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.CreateAsync(item);
    }

    public async Task RecordSessionJoinedAsync(
        Guid userId, Guid sessionId, string sessionName, bool isPublic = true)
    {
        var item = new ActivityFeedItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActivityType = "session_joined",
            RelatedEntityId = sessionId,
            Metadata = JsonSerializer.Serialize(new { sessionName }),
            IsPublic = isPublic,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.CreateAsync(item);
    }

    public async Task RecordAchievementUnlockedAsync(
        Guid userId, Guid userAchievementId, string achievementName, bool isPublic = true)
    {
        var item = new ActivityFeedItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActivityType = "achievement_unlocked",
            RelatedEntityId = userAchievementId,
            Metadata = JsonSerializer.Serialize(new { achievementName }),
            IsPublic = isPublic,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.CreateAsync(item);
    }
}
```

### DTOs

**`DTOs/ActivityFeed/ActivityFeedItemResponse.cs`**
```csharp
namespace RoundsApp.DTOs.ActivityFeed;

public class ActivityFeedItemResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserResponse? User { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public Guid RelatedEntityId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**`DTOs/ActivityFeed/ActivityFeedResponse.cs`**
```csharp
namespace RoundsApp.DTOs.ActivityFeed;

public class ActivityFeedResponse
{
    public List<ActivityFeedItemResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
    public bool HasMore { get; set; }
}
```

### Endpoints

**`Endpoints/ActivityFeedEndpoints.cs`**
```csharp
namespace RoundsApp.Endpoints;

public static class ActivityFeedEndpoints
{
    public static void MapActivityFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var feedApi = app.MapGroup("/api/activity-feed")
            .WithTags("Activity Feed")
            .RequireAuthorization();

        feedApi.MapGet("/", GetFriendsFeed);
        feedApi.MapGet("/me", GetMyActivities);
        feedApi.MapGet("/user/{userId:guid}", GetUserActivities);
        feedApi.MapDelete("/{id:guid}", DeleteActivity);
    }

    private static async Task<IResult> GetFriendsFeed(
        [FromQuery] int limit,
        [FromQuery] int offset,
        ClaimsPrincipal user,
        IActivityFeedRepository activityFeedRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        limit = Math.Clamp(limit == 0 ? 50 : limit, 1, 100);
        offset = Math.Max(offset, 0);

        var items = await activityFeedRepository.GetFriendsFeedAsync(
            currentUser.Id, limit + 1, offset);

        var itemsList = items.ToList();
        var hasMore = itemsList.Count > limit;
        if (hasMore) itemsList = itemsList.Take(limit).ToList();

        return Results.Ok(new ActivityFeedResponse
        {
            Items = itemsList.Select(ToResponse).ToList(),
            TotalCount = itemsList.Count,
            Offset = offset,
            Limit = limit,
            HasMore = hasMore
        });
    }

    private static async Task<IResult> GetMyActivities(
        [FromQuery] int limit,
        [FromQuery] int offset,
        ClaimsPrincipal user,
        IActivityFeedRepository activityFeedRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        limit = Math.Clamp(limit == 0 ? 50 : limit, 1, 100);
        offset = Math.Max(offset, 0);

        var items = await activityFeedRepository.GetByUserIdAsync(
            currentUser.Id, limit, offset);

        return Results.Ok(new ActivityFeedResponse
        {
            Items = items.Select(ToResponse).ToList(),
            TotalCount = items.Count(),
            Offset = offset,
            Limit = limit
        });
    }

    private static async Task<IResult> GetUserActivities(
        Guid userId,
        [FromQuery] int limit,
        [FromQuery] int offset,
        ClaimsPrincipal user,
        IActivityFeedRepository activityFeedRepository,
        IFriendshipRepository friendshipRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        // Check if users are friends
        var friendship = await friendshipRepository.GetByIdAsync(currentUser.Id, userId);
        if (friendship == null || friendship.Status != "accepted")
        {
            // Return only public activities if not friends
        }

        limit = Math.Clamp(limit == 0 ? 50 : limit, 1, 100);
        offset = Math.Max(offset, 0);

        var items = await activityFeedRepository.GetPublicFeedByUserIdAsync(
            userId, limit, offset);

        return Results.Ok(new ActivityFeedResponse
        {
            Items = items.Select(ToResponse).ToList(),
            TotalCount = items.Count(),
            Offset = offset,
            Limit = limit
        });
    }

    private static async Task<IResult> DeleteActivity(
        Guid id,
        ClaimsPrincipal user,
        IActivityFeedRepository activityFeedRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var item = await activityFeedRepository.GetByIdAsync(id);
        if (item == null) return Results.NotFound();
        if (item.UserId != currentUser.Id) return Results.Forbid();

        await activityFeedRepository.DeleteAsync(id);
        return Results.NoContent();
    }

    private static ActivityFeedItemResponse ToResponse(ActivityFeedItem item)
    {
        return new ActivityFeedItemResponse
        {
            Id = item.Id,
            UserId = item.UserId,
            User = item.User != null ? new UserResponse
            {
                Id = item.User.Id,
                UserName = item.User.UserName
            } : null,
            ActivityType = item.ActivityType,
            RelatedEntityId = item.RelatedEntityId,
            Metadata = item.Metadata,
            CreatedAt = item.CreatedAt
        };
    }
}
```

## Modifications to Existing Files

### SessionEndpoints.cs
In `CreateSession`, after creating the session:
```csharp
await activityFeedService.RecordSessionCreatedAsync(
    currentUser.Id,
    created.Id,
    created.Name);
```

### SessionParticipantEndpoints.cs
In `CreateParticipant`, after adding the participant:
```csharp
var session = await sessionRepository.GetByIdAsync(request.SessionId);
await activityFeedService.RecordSessionJoinedAsync(
    request.UserId,
    session.Id,
    session.Name);
```

### UserAchievementEndpoints.cs (or wherever achievements are unlocked)
After unlocking an achievement:
```csharp
await activityFeedService.RecordAchievementUnlockedAsync(
    userId,
    userAchievement.Id,
    achievement.Name);
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/activity-feed` | Get friends' activities (paginated) |
| GET | `/api/activity-feed/me` | Get own activity history |
| GET | `/api/activity-feed/user/{userId}` | Get specific user's public activities |
| DELETE | `/api/activity-feed/{id}` | Delete own activity |

### Query Parameters
- `limit` - Number of items per page (default: 50, max: 100)
- `offset` - Number of items to skip (default: 0)

## Database Migration

```sql
CREATE TABLE "ActivityFeedItems" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    "ActivityType" varchar(50) NOT NULL,
    "RelatedEntityId" uuid NOT NULL,
    "Metadata" jsonb,
    "IsPublic" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL
);

CREATE INDEX "IX_ActivityFeedItems_UserId" ON "ActivityFeedItems"("UserId");
CREATE INDEX "IX_ActivityFeedItems_CreatedAt" ON "ActivityFeedItems"("CreatedAt" DESC);
CREATE INDEX "IX_ActivityFeedItems_UserId_IsPublic" ON "ActivityFeedItems"("UserId", "IsPublic");
CREATE INDEX "IX_ActivityFeedItems_ActivityType" ON "ActivityFeedItems"("ActivityType");
```

## DbContext Configuration

```csharp
builder.Entity<ActivityFeedItem>(entity =>
{
    entity.HasOne(a => a.User)
        .WithMany()
        .HasForeignKey(a => a.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(a => a.CreatedAt).IsDescending();
    entity.HasIndex(a => new { a.UserId, a.IsPublic });
});
```

## Metadata Examples

**session_created:**
```json
{ "sessionName": "Friday Night Drinks" }
```

**session_joined:**
```json
{ "sessionName": "Friday Night Drinks" }
```

**achievement_unlocked:**
```json
{ "achievementName": "First Timer" }
```

## Privacy Rules
1. `IsPublic = true` - Activity visible to friends
2. `IsPublic = false` - Activity visible only to self
3. Non-friends can only see public activities
4. Users can delete their own activities

## Testing
1. Create session - verify activity recorded
2. Join session - verify activity recorded
3. Unlock achievement - verify activity recorded
4. Get friends feed - verify only friends' public activities shown
5. Get non-friend's activities - verify only public shown
6. Delete own activity - verify removed
7. Test pagination with limit/offset
