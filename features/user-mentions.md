# Feature: User Mentions in Comments

## Overview
Parse @username patterns in session comments, link mentions to users, and send notifications when users are mentioned.

## Priority
**3rd** - Depends on real-time notifications infrastructure

## New Files

### Model

**`Models/CommentMention.cs`**
```csharp
namespace RoundsApp.Models;

public class CommentMention
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CommentId { get; set; }

    [ForeignKey(nameof(CommentId))]
    public SessionComment? Comment { get; set; }

    [Required]
    public Guid MentionedUserId { get; set; }

    [ForeignKey(nameof(MentionedUserId))]
    public ApplicationUser? MentionedUser { get; set; }

    [Required]
    public int StartPosition { get; set; }

    [Required]
    public int Length { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Repository Interface

**`Repositories/IRepositories/ICommentMentionRepository.cs`**
```csharp
namespace RoundsApp.Repositories.IRepositories;

public interface ICommentMentionRepository
{
    Task<CommentMention?> GetByIdAsync(Guid id);
    Task<IEnumerable<CommentMention>> GetByCommentIdAsync(Guid commentId);
    Task<IEnumerable<CommentMention>> GetByUserIdAsync(Guid userId);
    Task<CommentMention> CreateAsync(CommentMention mention);
    Task CreateMultipleAsync(IEnumerable<CommentMention> mentions);
    Task<bool> DeleteByCommentIdAsync(Guid commentId);
}
```

### Repository Implementation

**`Repositories/CommentMentionRepository.cs`**
```csharp
namespace RoundsApp.Repositories;

public class CommentMentionRepository : ICommentMentionRepository
{
    private readonly ApplicationDbContext _context;

    public CommentMentionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommentMention>> GetByCommentIdAsync(Guid commentId)
    {
        return await _context.CommentMentions
            .Include(m => m.MentionedUser)
            .Where(m => m.CommentId == commentId)
            .OrderBy(m => m.StartPosition)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommentMention>> GetByUserIdAsync(Guid userId)
    {
        return await _context.CommentMentions
            .Include(m => m.Comment)
                .ThenInclude(c => c!.Session)
            .Include(m => m.Comment)
                .ThenInclude(c => c!.CreatedBy)
            .Where(m => m.MentionedUserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateMultipleAsync(IEnumerable<CommentMention> mentions)
    {
        await _context.CommentMentions.AddRangeAsync(mentions);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteByCommentIdAsync(Guid commentId)
    {
        var mentions = await _context.CommentMentions
            .Where(m => m.CommentId == commentId)
            .ToListAsync();

        _context.CommentMentions.RemoveRange(mentions);
        await _context.SaveChangesAsync();
        return true;
    }

    // ... other methods
}
```

### Mention Service Interface

**`Services/IMentionService.cs`**
```csharp
namespace RoundsApp.Services;

public interface IMentionService
{
    Task<IEnumerable<CommentMention>> ParseAndCreateMentionsAsync(
        Guid commentId,
        string content,
        Guid createdById);
    IEnumerable<MentionParseResult> ParseMentions(string content);
}

public class MentionParseResult
{
    public string Username { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int Length { get; set; }
}
```

### Mention Service Implementation

**`Services/MentionService.cs`**
```csharp
namespace RoundsApp.Services;

public class MentionService : IMentionService
{
    private static readonly Regex MentionPattern = new(@"@(\w+)", RegexOptions.Compiled);

    private readonly ICommentMentionRepository _mentionRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public MentionService(
        ICommentMentionRepository mentionRepository,
        UserManager<ApplicationUser> userManager)
    {
        _mentionRepository = mentionRepository;
        _userManager = userManager;
    }

    public IEnumerable<MentionParseResult> ParseMentions(string content)
    {
        var matches = MentionPattern.Matches(content);
        return matches.Select(m => new MentionParseResult
        {
            Username = m.Groups[1].Value,
            StartPosition = m.Index,
            Length = m.Length
        }).ToList();
    }

    public async Task<IEnumerable<CommentMention>> ParseAndCreateMentionsAsync(
        Guid commentId,
        string content,
        Guid createdById)
    {
        var parseResults = ParseMentions(content);
        var mentions = new List<CommentMention>();

        foreach (var result in parseResults)
        {
            var user = await _userManager.FindByNameAsync(result.Username);
            if (user != null)
            {
                mentions.Add(new CommentMention
                {
                    Id = Guid.NewGuid(),
                    CommentId = commentId,
                    MentionedUserId = user.Id,
                    StartPosition = result.StartPosition,
                    Length = result.Length,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (mentions.Any())
        {
            await _mentionRepository.CreateMultipleAsync(mentions);
        }

        return mentions;
    }
}
```

### DTO

**`DTOs/Sessions/CommentMentionResponse.cs`**
```csharp
namespace RoundsApp.DTOs.Sessions;

public class CommentMentionResponse
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid MentionedUserId { get; set; }
    public UserResponse? MentionedUser { get; set; }
    public int StartPosition { get; set; }
    public int Length { get; set; }
}
```

## Modifications to Existing Files

### Models/SessionComment.cs
Add navigation property:
```csharp
public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
```

### DTOs/Sessions/CommentResponse.cs
Add mentions list:
```csharp
public List<CommentMentionResponse> Mentions { get; set; } = new();
```

### Endpoints/SessionCommentEndpoints.cs

**Modify CreateComment:**
```csharp
private static async Task<IResult> CreateComment(
    CreateCommentRequest request,
    ClaimsPrincipal user,
    ISessionCommentRepository commentRepository,
    IMentionService mentionService,
    INotificationService notificationService,
    IDrinkingSessionRepository sessionRepository,
    UserManager<ApplicationUser> userManager)
{
    var currentUser = await userManager.GetUserAsync(user);
    if (currentUser == null) return Results.Unauthorized();

    var session = await sessionRepository.GetByIdAsync(request.SessionId);
    if (session == null) return Results.NotFound();

    var comment = new SessionComment
    {
        Id = Guid.NewGuid(),
        SessionId = request.SessionId,
        Content = request.Content,
        CreatedById = currentUser.Id,
        CreatedAt = DateTime.UtcNow
    };

    var created = await commentRepository.CreateAsync(comment);

    // Parse and create mentions
    var mentions = await mentionService.ParseAndCreateMentionsAsync(
        created.Id,
        created.Content,
        currentUser.Id);

    // Send notifications to mentioned users
    foreach (var mention in mentions)
    {
        if (mention.MentionedUserId != currentUser.Id)
        {
            await notificationService.CreateAndSendAsync(
                mention.MentionedUserId,
                "mention",
                "You were mentioned",
                $"{currentUser.UserName} mentioned you in a comment",
                JsonSerializer.Serialize(new {
                    commentId = created.Id,
                    sessionId = request.SessionId,
                    sessionName = session.Name
                })
            );
        }
    }

    return Results.Created($"/api/session-comments/{created.Id}", ToResponse(created, mentions));
}
```

**Modify UpdateComment:**
```csharp
private static async Task<IResult> UpdateComment(
    Guid id,
    UpdateCommentRequest request,
    ClaimsPrincipal user,
    ISessionCommentRepository commentRepository,
    ICommentMentionRepository mentionRepository,
    IMentionService mentionService,
    INotificationService notificationService,
    IDrinkingSessionRepository sessionRepository,
    UserManager<ApplicationUser> userManager)
{
    var currentUser = await userManager.GetUserAsync(user);
    if (currentUser == null) return Results.Unauthorized();

    var comment = await commentRepository.GetByIdAsync(id);
    if (comment == null) return Results.NotFound();
    if (comment.CreatedById != currentUser.Id) return Results.Forbid();

    // Get old mentions for comparison
    var oldMentions = await mentionRepository.GetByCommentIdAsync(id);
    var oldMentionedUserIds = oldMentions.Select(m => m.MentionedUserId).ToHashSet();

    // Update comment content
    if (!string.IsNullOrEmpty(request.Content))
    {
        comment.Content = request.Content;
    }
    comment.UpdatedById = currentUser.Id;
    comment.UpdatedAt = DateTime.UtcNow;

    await commentRepository.UpdateAsync(comment);

    // Delete old mentions and create new ones
    await mentionRepository.DeleteByCommentIdAsync(id);
    var newMentions = await mentionService.ParseAndCreateMentionsAsync(
        id,
        comment.Content,
        currentUser.Id);

    // Notify newly mentioned users only
    var session = await sessionRepository.GetByIdAsync(comment.SessionId);
    foreach (var mention in newMentions)
    {
        if (mention.MentionedUserId != currentUser.Id &&
            !oldMentionedUserIds.Contains(mention.MentionedUserId))
        {
            await notificationService.CreateAndSendAsync(
                mention.MentionedUserId,
                "mention",
                "You were mentioned",
                $"{currentUser.UserName} mentioned you in a comment",
                JsonSerializer.Serialize(new {
                    commentId = id,
                    sessionId = comment.SessionId,
                    sessionName = session?.Name
                })
            );
        }
    }

    return Results.Ok(ToResponse(comment, newMentions));
}
```

**Update ToResponse helper:**
```csharp
private static CommentResponse ToResponse(SessionComment comment, IEnumerable<CommentMention>? mentions = null)
{
    return new CommentResponse
    {
        Id = comment.Id,
        SessionId = comment.SessionId,
        Content = comment.Content,
        CreatedById = comment.CreatedById,
        CreatedBy = comment.CreatedBy != null ? new UserResponse
        {
            Id = comment.CreatedBy.Id,
            UserName = comment.CreatedBy.UserName
        } : null,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        Mentions = mentions?.Select(m => new CommentMentionResponse
        {
            Id = m.Id,
            CommentId = m.CommentId,
            MentionedUserId = m.MentionedUserId,
            MentionedUser = m.MentionedUser != null ? new UserResponse
            {
                Id = m.MentionedUser.Id,
                UserName = m.MentionedUser.UserName
            } : null,
            StartPosition = m.StartPosition,
            Length = m.Length
        }).ToList() ?? new List<CommentMentionResponse>()
    };
}
```

## Database Migration

```sql
CREATE TABLE "CommentMentions" (
    "Id" uuid PRIMARY KEY,
    "CommentId" uuid NOT NULL REFERENCES "SessionComments"("Id") ON DELETE CASCADE,
    "MentionedUserId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    "StartPosition" integer NOT NULL,
    "Length" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);

CREATE INDEX "IX_CommentMentions_CommentId" ON "CommentMentions"("CommentId");
CREATE INDEX "IX_CommentMentions_MentionedUserId" ON "CommentMentions"("MentionedUserId");
```

## DbContext Configuration

```csharp
builder.Entity<CommentMention>(entity =>
{
    entity.HasOne(m => m.Comment)
        .WithMany(c => c.Mentions)
        .HasForeignKey(m => m.CommentId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(m => m.MentionedUser)
        .WithMany()
        .HasForeignKey(m => m.MentionedUserId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

## Mention Pattern
- Regex: `@(\w+)`
- Matches: `@john`, `@user123`, `@JaneDoe`
- Does not match: `@`, `@ john`, `email@example.com`

## Notification Metadata
```json
{
  "commentId": "guid",
  "sessionId": "guid",
  "sessionName": "Friday Night Drinks"
}
```

## Testing
1. Create comment with `@username` - verify mention created
2. Create comment with non-existent `@fake` - verify no mention
3. Verify notification sent to mentioned user
4. Self-mention (`@myself`) - verify no notification
5. Update comment adding new mention - verify notification to new user only
6. Update comment removing mention - verify mention deleted
7. Delete comment - verify cascade deletes mentions
