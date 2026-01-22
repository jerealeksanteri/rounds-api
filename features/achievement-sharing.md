# Feature: Achievement Sharing (Deep Links)

## Overview
Generate shareable deep links for achievements that can be opened in the mobile app or viewed on the web. Track share events and link views.

## Priority
**6th** - Uses existing achievement infrastructure

## Deep Link Format
- **App Deep Link:** `roundsapp://achievement/{token}`
- **Web Fallback:** `https://app.rounds.com/share/achievement/{token}`

## New Files

### Model

**`Models/AchievementShare.cs`**
```csharp
namespace RoundsApp.Models;

public class AchievementShare
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserAchievementId { get; set; }

    [ForeignKey(nameof(UserAchievementId))]
    public UserAchievement? UserAchievement { get; set; }

    [Required]
    public Guid SharedByUserId { get; set; }

    [ForeignKey(nameof(SharedByUserId))]
    public ApplicationUser? SharedBy { get; set; }

    [Required]
    [MaxLength(50)]
    public string ShareToken { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Platform { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public int ViewCount { get; set; } = 0;
}
```

### Repository Interface

**`Repositories/IRepositories/IAchievementShareRepository.cs`**
```csharp
namespace RoundsApp.Repositories.IRepositories;

public interface IAchievementShareRepository
{
    Task<AchievementShare?> GetByIdAsync(Guid id);
    Task<AchievementShare?> GetByTokenAsync(string token);
    Task<IEnumerable<AchievementShare>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AchievementShare>> GetByUserAchievementIdAsync(Guid userAchievementId);
    Task<AchievementShare> CreateAsync(AchievementShare share);
    Task<bool> DeleteAsync(Guid id);
    Task IncrementViewCountAsync(string token);
}
```

### Repository Implementation

**`Repositories/AchievementShareRepository.cs`**
```csharp
namespace RoundsApp.Repositories;

public class AchievementShareRepository : IAchievementShareRepository
{
    private readonly ApplicationDbContext _context;

    public AchievementShareRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AchievementShare?> GetByIdAsync(Guid id)
    {
        return await _context.AchievementShares
            .Include(s => s.UserAchievement)
                .ThenInclude(ua => ua!.Achievement)
            .Include(s => s.UserAchievement)
                .ThenInclude(ua => ua!.User)
            .Include(s => s.SharedBy)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<AchievementShare?> GetByTokenAsync(string token)
    {
        return await _context.AchievementShares
            .Include(s => s.UserAchievement)
                .ThenInclude(ua => ua!.Achievement)
            .Include(s => s.UserAchievement)
                .ThenInclude(ua => ua!.User)
            .Include(s => s.SharedBy)
            .FirstOrDefaultAsync(s => s.ShareToken == token);
    }

    public async Task<IEnumerable<AchievementShare>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AchievementShares
            .Include(s => s.UserAchievement)
                .ThenInclude(ua => ua!.Achievement)
            .Where(s => s.SharedByUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AchievementShare>> GetByUserAchievementIdAsync(Guid userAchievementId)
    {
        return await _context.AchievementShares
            .Where(s => s.UserAchievementId == userAchievementId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(string token)
    {
        var share = await _context.AchievementShares
            .FirstOrDefaultAsync(s => s.ShareToken == token);

        if (share != null)
        {
            share.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AchievementShare> CreateAsync(AchievementShare share)
    {
        _context.AchievementShares.Add(share);
        await _context.SaveChangesAsync();
        return share;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var share = await _context.AchievementShares.FindAsync(id);
        if (share == null) return false;

        _context.AchievementShares.Remove(share);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

### Deep Link Service Interface

**`Services/IDeepLinkService.cs`**
```csharp
namespace RoundsApp.Services;

public interface IDeepLinkService
{
    Task<AchievementShareResult> GenerateAchievementShareLinkAsync(
        Guid userAchievementId,
        Guid userId,
        string? platform = null);
    string GenerateShareToken();
    string BuildAppDeepLink(string token);
    string BuildWebLink(string token);
}

public class AchievementShareResult
{
    public Guid ShareId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string AppDeepLink { get; set; } = string.Empty;
    public string WebLink { get; set; } = string.Empty;
}
```

### Deep Link Service Implementation

**`Services/DeepLinkService.cs`**
```csharp
namespace RoundsApp.Services;

public class DeepLinkService : IDeepLinkService
{
    private readonly IConfiguration _configuration;
    private readonly IAchievementShareRepository _shareRepository;

    public DeepLinkService(
        IConfiguration configuration,
        IAchievementShareRepository shareRepository)
    {
        _configuration = configuration;
        _shareRepository = shareRepository;
    }

    public string GenerateShareToken()
    {
        // Generate URL-safe unique token
        var bytes = Guid.NewGuid().ToByteArray();
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public string BuildAppDeepLink(string token)
    {
        var baseUrl = _configuration["DeepLink:BaseUrl"] ?? "roundsapp://";
        return $"{baseUrl}achievement/{token}";
    }

    public string BuildWebLink(string token)
    {
        var webUrl = _configuration["DeepLink:WebFallbackUrl"] ?? "https://app.rounds.com/share";
        return $"{webUrl}/achievement/{token}";
    }

    public async Task<AchievementShareResult> GenerateAchievementShareLinkAsync(
        Guid userAchievementId,
        Guid userId,
        string? platform = null)
    {
        var token = GenerateShareToken();

        var share = new AchievementShare
        {
            Id = Guid.NewGuid(),
            UserAchievementId = userAchievementId,
            SharedByUserId = userId,
            ShareToken = token,
            Platform = platform,
            CreatedAt = DateTime.UtcNow,
            ViewCount = 0
        };

        await _shareRepository.CreateAsync(share);

        return new AchievementShareResult
        {
            ShareId = share.Id,
            Token = token,
            AppDeepLink = BuildAppDeepLink(token),
            WebLink = BuildWebLink(token)
        };
    }
}
```

### DTOs

**`DTOs/Achievements/CreateAchievementShareRequest.cs`**
```csharp
namespace RoundsApp.DTOs.Achievements;

public class CreateAchievementShareRequest
{
    [Required]
    public Guid UserAchievementId { get; set; }

    [MaxLength(50)]
    public string? Platform { get; set; }
}
```

**`DTOs/Achievements/AchievementShareResponse.cs`**
```csharp
namespace RoundsApp.DTOs.Achievements;

public class AchievementShareResponse
{
    public Guid Id { get; set; }
    public Guid UserAchievementId { get; set; }
    public string ShareToken { get; set; } = string.Empty;
    public string AppDeepLink { get; set; } = string.Empty;
    public string WebLink { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewCount { get; set; }
}
```

**`DTOs/Achievements/SharedAchievementViewResponse.cs`**
```csharp
namespace RoundsApp.DTOs.Achievements;

public class SharedAchievementViewResponse
{
    public Guid AchievementId { get; set; }
    public string AchievementName { get; set; } = string.Empty;
    public string AchievementDescription { get; set; } = string.Empty;
    public string? AchievementIcon { get; set; }
    public string AchievementType { get; set; } = string.Empty;
    public Guid SharedByUserId { get; set; }
    public string SharedByUserName { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
    public DateTime SharedAt { get; set; }
}
```

### Endpoints

**`Endpoints/AchievementShareEndpoints.cs`**
```csharp
namespace RoundsApp.Endpoints;

public static class AchievementShareEndpoints
{
    public static void MapAchievementShareEndpoints(this IEndpointRouteBuilder app)
    {
        var shareApi = app.MapGroup("/api/achievement-shares")
            .WithTags("Achievement Sharing");

        // Public endpoint - no auth required
        shareApi.MapGet("/view/{token}", ViewSharedAchievement)
            .WithName("ViewSharedAchievement")
            .WithOpenApi();

        // Protected endpoints
        var protectedApi = shareApi.RequireAuthorization();
        protectedApi.MapPost("/", CreateShare);
        protectedApi.MapGet("/me", GetMyShares);
        protectedApi.MapGet("/{id:guid}", GetShareById);
        protectedApi.MapDelete("/{id:guid}", DeleteShare);
    }

    private static async Task<IResult> ViewSharedAchievement(
        string token,
        IAchievementShareRepository shareRepository)
    {
        var share = await shareRepository.GetByTokenAsync(token);
        if (share == null) return Results.NotFound(new { error = "Share link not found or expired" });

        // Check expiration
        if (share.ExpiresAt.HasValue && share.ExpiresAt < DateTime.UtcNow)
        {
            return Results.NotFound(new { error = "Share link has expired" });
        }

        // Increment view count
        await shareRepository.IncrementViewCountAsync(token);

        var achievement = share.UserAchievement?.Achievement;
        var sharedBy = share.SharedBy;

        return Results.Ok(new SharedAchievementViewResponse
        {
            AchievementId = achievement?.Id ?? Guid.Empty,
            AchievementName = achievement?.Name ?? string.Empty,
            AchievementDescription = achievement?.Description ?? string.Empty,
            AchievementIcon = achievement?.Icon,
            AchievementType = achievement?.Type ?? string.Empty,
            SharedByUserId = sharedBy?.Id ?? Guid.Empty,
            SharedByUserName = sharedBy?.UserName ?? string.Empty,
            UnlockedAt = share.UserAchievement?.UnlockedAt ?? DateTime.MinValue,
            SharedAt = share.CreatedAt
        });
    }

    private static async Task<IResult> CreateShare(
        CreateAchievementShareRequest request,
        ClaimsPrincipal user,
        IDeepLinkService deepLinkService,
        IUserAchievementRepository userAchievementRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        // Verify user owns this achievement
        var userAchievement = await userAchievementRepository.GetByIdAsync(request.UserAchievementId);
        if (userAchievement == null) return Results.NotFound(new { error = "Achievement not found" });
        if (userAchievement.UserId != currentUser.Id) return Results.Forbid();

        var result = await deepLinkService.GenerateAchievementShareLinkAsync(
            request.UserAchievementId,
            currentUser.Id,
            request.Platform);

        return Results.Created($"/api/achievement-shares/{result.ShareId}", new AchievementShareResponse
        {
            Id = result.ShareId,
            UserAchievementId = request.UserAchievementId,
            ShareToken = result.Token,
            AppDeepLink = result.AppDeepLink,
            WebLink = result.WebLink,
            Platform = request.Platform,
            CreatedAt = DateTime.UtcNow,
            ViewCount = 0
        });
    }

    private static async Task<IResult> GetMyShares(
        ClaimsPrincipal user,
        IAchievementShareRepository shareRepository,
        IDeepLinkService deepLinkService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var shares = await shareRepository.GetByUserIdAsync(currentUser.Id);

        return Results.Ok(shares.Select(s => new AchievementShareResponse
        {
            Id = s.Id,
            UserAchievementId = s.UserAchievementId,
            ShareToken = s.ShareToken,
            AppDeepLink = deepLinkService.BuildAppDeepLink(s.ShareToken),
            WebLink = deepLinkService.BuildWebLink(s.ShareToken),
            Platform = s.Platform,
            CreatedAt = s.CreatedAt,
            ExpiresAt = s.ExpiresAt,
            ViewCount = s.ViewCount
        }));
    }

    private static async Task<IResult> GetShareById(
        Guid id,
        ClaimsPrincipal user,
        IAchievementShareRepository shareRepository,
        IDeepLinkService deepLinkService,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var share = await shareRepository.GetByIdAsync(id);
        if (share == null) return Results.NotFound();
        if (share.SharedByUserId != currentUser.Id) return Results.Forbid();

        return Results.Ok(new AchievementShareResponse
        {
            Id = share.Id,
            UserAchievementId = share.UserAchievementId,
            ShareToken = share.ShareToken,
            AppDeepLink = deepLinkService.BuildAppDeepLink(share.ShareToken),
            WebLink = deepLinkService.BuildWebLink(share.ShareToken),
            Platform = share.Platform,
            CreatedAt = share.CreatedAt,
            ExpiresAt = share.ExpiresAt,
            ViewCount = share.ViewCount
        });
    }

    private static async Task<IResult> DeleteShare(
        Guid id,
        ClaimsPrincipal user,
        IAchievementShareRepository shareRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var share = await shareRepository.GetByIdAsync(id);
        if (share == null) return Results.NotFound();
        if (share.SharedByUserId != currentUser.Id) return Results.Forbid();

        await shareRepository.DeleteAsync(id);
        return Results.NoContent();
    }
}
```

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/achievement-shares` | Required | Create share link |
| GET | `/api/achievement-shares/view/{token}` | **None** | View shared achievement (public) |
| GET | `/api/achievement-shares/me` | Required | Get my share history |
| GET | `/api/achievement-shares/{id}` | Required | Get specific share |
| DELETE | `/api/achievement-shares/{id}` | Required | Delete share link |

## Configuration (appsettings.json)

```json
{
  "DeepLink": {
    "BaseUrl": "roundsapp://",
    "WebFallbackUrl": "https://app.rounds.com/share"
  }
}
```

## Database Migration

```sql
CREATE TABLE "AchievementShares" (
    "Id" uuid PRIMARY KEY,
    "UserAchievementId" uuid NOT NULL REFERENCES "UserAchievements"("Id") ON DELETE CASCADE,
    "SharedByUserId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    "ShareToken" varchar(50) NOT NULL,
    "Platform" varchar(50),
    "CreatedAt" timestamp with time zone NOT NULL,
    "ExpiresAt" timestamp with time zone,
    "ViewCount" integer NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX "IX_AchievementShares_ShareToken" ON "AchievementShares"("ShareToken");
CREATE INDEX "IX_AchievementShares_SharedByUserId" ON "AchievementShares"("SharedByUserId");
CREATE INDEX "IX_AchievementShares_UserAchievementId" ON "AchievementShares"("UserAchievementId");
```

## DbContext Configuration

```csharp
builder.Entity<AchievementShare>(entity =>
{
    entity.HasIndex(s => s.ShareToken).IsUnique();

    entity.HasOne(s => s.UserAchievement)
        .WithMany()
        .HasForeignKey(s => s.UserAchievementId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(s => s.SharedBy)
        .WithMany()
        .HasForeignKey(s => s.SharedByUserId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

## Token Format
URL-safe Base64 encoded GUID:
- Example: `YWJjZGVmZ2hpamtsbW5vcA`
- Length: ~22 characters
- Characters: `A-Z`, `a-z`, `0-9`, `-`, `_`

## Platform Tracking
Optional field to track where users share:
- `twitter`
- `facebook`
- `instagram`
- `whatsapp`
- `copy` (copied to clipboard)
- `other`

## Response Examples

**Create Share Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userAchievementId": "7fa85f64-5717-4562-b3fc-2c963f66afa9",
  "shareToken": "YWJjZGVmZ2hpamtsbW5vcA",
  "appDeepLink": "roundsapp://achievement/YWJjZGVmZ2hpamtsbW5vcA",
  "webLink": "https://app.rounds.com/share/achievement/YWJjZGVmZ2hpamtsbW5vcA",
  "platform": "twitter",
  "createdAt": "2026-01-22T12:00:00Z",
  "viewCount": 0
}
```

**View Shared Achievement (Public):**
```json
{
  "achievementId": "5fa85f64-5717-4562-b3fc-2c963f66afa7",
  "achievementName": "First Timer",
  "achievementDescription": "Joined your first drinking session",
  "achievementIcon": "🍺",
  "achievementType": "milestone",
  "sharedByUserId": "1fa85f64-5717-4562-b3fc-2c963f66afa1",
  "sharedByUserName": "john_doe",
  "unlockedAt": "2026-01-15T20:30:00Z",
  "sharedAt": "2026-01-22T12:00:00Z"
}
```

## Mobile App Integration

### iOS (Swift)
```swift
// Handle deep link in AppDelegate or SceneDelegate
func application(_ app: UIApplication, open url: URL, options: [UIApplication.OpenURLOptionsKey: Any] = [:]) -> Bool {
    if url.scheme == "roundsapp" && url.host == "achievement" {
        let token = url.lastPathComponent
        // Navigate to achievement view with token
    }
    return true
}
```

### Android (Kotlin)
```kotlin
// In AndroidManifest.xml
<intent-filter>
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="roundsapp" android:host="achievement" />
</intent-filter>

// Handle in Activity
override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    intent.data?.let { uri ->
        val token = uri.lastPathSegment
        // Navigate to achievement view with token
    }
}
```

## Testing
1. Create share link - verify links generated
2. View with valid token - verify achievement data returned
3. View with invalid token - verify 404
4. View increments counter - verify ViewCount increases
5. Delete share - verify removed
6. Try to share another user's achievement - verify 403
7. Get my shares - verify list with view counts
