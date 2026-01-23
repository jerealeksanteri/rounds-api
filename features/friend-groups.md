# Feature: Friend Groups

## Overview
Create named groups of friends for easier session management. Users can add existing friends to groups and bulk-invite entire groups to sessions.

## Priority
**2nd** - Independent feature that enables bulk operations

## New Files

### Models

**`Models/FriendGroup.cs`**
```csharp
namespace RoundsApp.Models;

public class FriendGroup
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser? Owner { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }

    [ForeignKey(nameof(UpdatedById))]
    public ApplicationUser? UpdatedBy { get; set; }

    public ICollection<FriendGroupMember> Members { get; set; } = new List<FriendGroupMember>();
}
```

**`Models/FriendGroupMember.cs`**
```csharp
namespace RoundsApp.Models;

[PrimaryKey(nameof(GroupId), nameof(UserId))]
public class FriendGroupMember
{
    [Required]
    public Guid GroupId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public FriendGroup? Group { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid AddedById { get; set; }

    [ForeignKey(nameof(AddedById))]
    public ApplicationUser? AddedBy { get; set; }
}
```

### Repository Interfaces

**`Repositories/IRepositories/IFriendGroupRepository.cs`**
```csharp
namespace RoundsApp.Repositories.IRepositories;

public interface IFriendGroupRepository
{
    Task<FriendGroup?> GetByIdAsync(Guid id);
    Task<IEnumerable<FriendGroup>> GetAllAsync();
    Task<FriendGroup> CreateAsync(FriendGroup group);
    Task<FriendGroup> UpdateAsync(FriendGroup group);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<FriendGroup>> GetByOwnerIdAsync(Guid ownerId);
}
```

**`Repositories/IRepositories/IFriendGroupMemberRepository.cs`**
```csharp
namespace RoundsApp.Repositories.IRepositories;

public interface IFriendGroupMemberRepository
{
    Task<FriendGroupMember?> GetByIdAsync(Guid groupId, Guid userId);
    Task<IEnumerable<FriendGroupMember>> GetByGroupIdAsync(Guid groupId);
    Task<FriendGroupMember> CreateAsync(FriendGroupMember member);
    Task<bool> DeleteAsync(Guid groupId, Guid userId);
    Task CreateMultipleAsync(IEnumerable<FriendGroupMember> members);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId);
}
```

### Repository Implementations

**`Repositories/FriendGroupRepository.cs`**
```csharp
namespace RoundsApp.Repositories;

public class FriendGroupRepository : IFriendGroupRepository
{
    private readonly ApplicationDbContext _context;

    public FriendGroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FriendGroup?> GetByIdAsync(Guid id)
    {
        return await _context.FriendGroups
            .Include(g => g.Owner)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<FriendGroup>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.FriendGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Where(g => g.OwnerId == ownerId)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    // ... other methods
}
```

**`Repositories/FriendGroupMemberRepository.cs`**
```csharp
namespace RoundsApp.Repositories;

public class FriendGroupMemberRepository : IFriendGroupMemberRepository
{
    private readonly ApplicationDbContext _context;

    public FriendGroupMemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FriendGroupMember>> GetByGroupIdAsync(Guid groupId)
    {
        return await _context.FriendGroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.User!.UserName)
            .ToListAsync();
    }

    public async Task CreateMultipleAsync(IEnumerable<FriendGroupMember> members)
    {
        await _context.FriendGroupMembers.AddRangeAsync(members);
        await _context.SaveChangesAsync();
    }

    // ... other methods
}
```

### Validation Service

**`Services/IFriendGroupValidationService.cs`**
```csharp
namespace RoundsApp.Services;

public interface IFriendGroupValidationService
{
    Task<bool> AreFriendsAsync(Guid userId, Guid friendId);
    Task<bool> AreAllFriendsAsync(Guid userId, IEnumerable<Guid> friendIds);
    Task<IEnumerable<Guid>> FilterNonFriendsAsync(Guid userId, IEnumerable<Guid> userIds);
}
```

**`Services/FriendGroupValidationService.cs`**
```csharp
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
            f.Status == "accepted");
    }

    public async Task<bool> AreAllFriendsAsync(Guid userId, IEnumerable<Guid> friendIds)
    {
        var friends = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
        var acceptedFriendIds = friends
            .Where(f => f.Status == "accepted")
            .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
            .ToHashSet();

        return friendIds.All(id => acceptedFriendIds.Contains(id));
    }

    public async Task<IEnumerable<Guid>> FilterNonFriendsAsync(Guid userId, IEnumerable<Guid> userIds)
    {
        var friends = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
        var acceptedFriendIds = friends
            .Where(f => f.Status == "accepted")
            .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
            .ToHashSet();

        return userIds.Where(id => !acceptedFriendIds.Contains(id));
    }
}
```

### DTOs

**`DTOs/FriendGroups/CreateFriendGroupRequest.cs`**
```csharp
namespace RoundsApp.DTOs.FriendGroups;

public class CreateFriendGroupRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<Guid>? InitialMemberIds { get; set; }
}
```

**`DTOs/FriendGroups/UpdateFriendGroupRequest.cs`**
```csharp
namespace RoundsApp.DTOs.FriendGroups;

public class UpdateFriendGroupRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
```

**`DTOs/FriendGroups/FriendGroupResponse.cs`**
```csharp
namespace RoundsApp.DTOs.FriendGroups;

public class FriendGroupResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int MemberCount { get; set; }
    public List<FriendGroupMemberResponse> Members { get; set; } = new();
}
```

**`DTOs/FriendGroups/FriendGroupMemberResponse.cs`**
```csharp
namespace RoundsApp.DTOs.FriendGroups;

public class FriendGroupMemberResponse
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public UserResponse? User { get; set; }
    public DateTime AddedAt { get; set; }
}
```

**`DTOs/FriendGroups/AddGroupMembersRequest.cs`**
```csharp
namespace RoundsApp.DTOs.FriendGroups;

public class AddGroupMembersRequest
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();
}
```

**`DTOs/Sessions/BulkInviteRequest.cs`**
```csharp
namespace RoundsApp.DTOs.Sessions;

public class BulkInviteRequest
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid FriendGroupId { get; set; }
}
```

### Endpoints

**`Endpoints/FriendGroupEndpoints.cs`**
```csharp
namespace RoundsApp.Endpoints;

public static class FriendGroupEndpoints
{
    public static void MapFriendGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var groupApi = app.MapGroup("/api/friend-groups")
            .WithTags("Friend Groups")
            .RequireAuthorization();

        groupApi.MapGet("/", GetMyGroups);
        groupApi.MapGet("/{id:guid}", GetGroupById);
        groupApi.MapPost("/", CreateGroup);
        groupApi.MapPut("/{id:guid}", UpdateGroup);
        groupApi.MapDelete("/{id:guid}", DeleteGroup);
        groupApi.MapPost("/{id:guid}/members", AddMembers);
        groupApi.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMember);
        groupApi.MapPost("/{id:guid}/invite-to-session", BulkInviteToSession);
    }

    private static async Task<IResult> CreateGroup(
        CreateFriendGroupRequest request,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        IFriendGroupMemberRepository memberRepository,
        IFriendGroupValidationService validationService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        // Validate initial members are friends
        if (request.InitialMemberIds?.Any() == true)
        {
            var nonFriends = await validationService.FilterNonFriendsAsync(
                currentUser.Id, request.InitialMemberIds);
            if (nonFriends.Any())
            {
                return Results.BadRequest(new {
                    error = "Some users are not your friends",
                    nonFriendIds = nonFriends
                });
            }
        }

        var group = new FriendGroup
        {
            Id = Guid.NewGuid(),
            OwnerId = currentUser.Id,
            Name = request.Name,
            Description = request.Description,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        var created = await groupRepository.CreateAsync(group);

        // Add initial members
        if (request.InitialMemberIds?.Any() == true)
        {
            var members = request.InitialMemberIds.Select(userId => new FriendGroupMember
            {
                GroupId = created.Id,
                UserId = userId,
                AddedById = currentUser.Id,
                AddedAt = DateTime.UtcNow
            });
            await memberRepository.CreateMultipleAsync(members);
        }

        return Results.Created($"/api/friend-groups/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> BulkInviteToSession(
        Guid id,
        BulkInviteRequest request,
        ClaimsPrincipal user,
        IFriendGroupRepository groupRepository,
        IFriendGroupMemberRepository memberRepository,
        ISessionInviteRepository inviteRepository,
        IDrinkingSessionRepository sessionRepository,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var group = await groupRepository.GetByIdAsync(id);
        if (group == null) return Results.NotFound();
        if (group.OwnerId != currentUser.Id) return Results.Forbid();

        var session = await sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null) return Results.NotFound(new { error = "Session not found" });

        var members = await memberRepository.GetByGroupIdAsync(id);
        var createdInvites = new List<SessionInvite>();

        foreach (var member in members)
        {
            var invite = new SessionInvite
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                UserId = member.UserId,
                Status = "pending",
                CreatedById = currentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var created = await inviteRepository.CreateAsync(invite);
            createdInvites.Add(created);

            await notificationService.CreateAndSendAsync(
                member.UserId,
                "session_invite",
                "New Session Invite",
                $"{currentUser.UserName} invited you to {session.Name}",
                JsonSerializer.Serialize(new { sessionId = session.Id, inviteId = created.Id })
            );
        }

        return Results.Ok(new {
            invitesSent = createdInvites.Count,
            groupName = group.Name,
            sessionName = session.Name
        });
    }

    // ... other endpoint methods
}
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/friend-groups` | Get all groups owned by current user |
| GET | `/api/friend-groups/{id}` | Get group with members |
| POST | `/api/friend-groups` | Create new group |
| PUT | `/api/friend-groups/{id}` | Update group name/description |
| DELETE | `/api/friend-groups/{id}` | Delete group |
| POST | `/api/friend-groups/{id}/members` | Add members to group |
| DELETE | `/api/friend-groups/{id}/members/{userId}` | Remove member |
| POST | `/api/friend-groups/{id}/invite-to-session` | Bulk invite group to session |

## Database Migration

```sql
CREATE TABLE "FriendGroups" (
    "Id" uuid PRIMARY KEY,
    "OwnerId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    "Name" varchar(100) NOT NULL,
    "Description" varchar(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedById" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    "UpdatedAt" timestamp with time zone,
    "UpdatedById" uuid REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT
);

CREATE TABLE "FriendGroupMembers" (
    "GroupId" uuid NOT NULL REFERENCES "FriendGroups"("Id") ON DELETE CASCADE,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "AddedAt" timestamp with time zone NOT NULL,
    "AddedById" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    PRIMARY KEY ("GroupId", "UserId")
);

CREATE INDEX "IX_FriendGroups_OwnerId" ON "FriendGroups"("OwnerId");
CREATE INDEX "IX_FriendGroupMembers_UserId" ON "FriendGroupMembers"("UserId");
```

## DbContext Configuration

```csharp
builder.Entity<FriendGroup>(entity =>
{
    entity.HasOne(g => g.Owner)
        .WithMany()
        .HasForeignKey(g => g.OwnerId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(g => g.CreatedBy)
        .WithMany()
        .HasForeignKey(g => g.CreatedById)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(g => g.UpdatedBy)
        .WithMany()
        .HasForeignKey(g => g.UpdatedById)
        .OnDelete(DeleteBehavior.Restrict);
});

builder.Entity<FriendGroupMember>(entity =>
{
    entity.HasKey(m => new { m.GroupId, m.UserId });

    entity.HasOne(m => m.Group)
        .WithMany(g => g.Members)
        .HasForeignKey(m => m.GroupId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(m => m.User)
        .WithMany()
        .HasForeignKey(m => m.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(m => m.AddedBy)
        .WithMany()
        .HasForeignKey(m => m.AddedById)
        .OnDelete(DeleteBehavior.Restrict);
});
```

## Validation Rules
1. Only group owner can modify/delete group
2. Only accepted friends can be added as members
3. Cannot add same user twice
4. Group name is required (max 100 chars)

## Testing
1. Create group with initial members
2. Verify non-friends cannot be added
3. Bulk invite to session
4. Verify all members receive notifications
5. Remove member, verify they're gone
6. Delete group, verify cascade to members
