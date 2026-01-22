# Feature: Drink Ratings & Reviews

## Overview
Allow users to rate drinks (1-5 stars) with optional review text. Each user can only rate a drink once, and the system calculates average ratings.

## Priority
**5th** - Independent feature, no dependencies on other new features

## New Files

### Model

**`Models/DrinkRating.cs`**
```csharp
namespace RoundsApp.Models;

public class DrinkRating
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DrinkId { get; set; }

    [ForeignKey(nameof(DrinkId))]
    public Drink? Drink { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Review { get; set; }

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
}
```

### Repository Interface

**`Repositories/IRepositories/IDrinkRatingRepository.cs`**
```csharp
namespace RoundsApp.Repositories.IRepositories;

public interface IDrinkRatingRepository
{
    Task<DrinkRating?> GetByIdAsync(Guid id);
    Task<IEnumerable<DrinkRating>> GetAllAsync();
    Task<DrinkRating> CreateAsync(DrinkRating rating);
    Task<DrinkRating> UpdateAsync(DrinkRating rating);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<DrinkRating>> GetByDrinkIdAsync(Guid drinkId);
    Task<IEnumerable<DrinkRating>> GetByUserIdAsync(Guid userId);
    Task<DrinkRating?> GetByUserAndDrinkIdAsync(Guid userId, Guid drinkId);
    Task<double> GetAverageRatingAsync(Guid drinkId);
    Task<int> GetRatingCountAsync(Guid drinkId);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid drinkId);
}
```

### Repository Implementation

**`Repositories/DrinkRatingRepository.cs`**
```csharp
namespace RoundsApp.Repositories;

public class DrinkRatingRepository : IDrinkRatingRepository
{
    private readonly ApplicationDbContext _context;

    public DrinkRatingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DrinkRating?> GetByIdAsync(Guid id)
    {
        return await _context.DrinkRatings
            .Include(r => r.User)
            .Include(r => r.Drink)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<DrinkRating>> GetByDrinkIdAsync(Guid drinkId)
    {
        return await _context.DrinkRatings
            .Include(r => r.User)
            .Where(r => r.DrinkId == drinkId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DrinkRating>> GetByUserIdAsync(Guid userId)
    {
        return await _context.DrinkRatings
            .Include(r => r.Drink)
                .ThenInclude(d => d!.DrinkType)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<DrinkRating?> GetByUserAndDrinkIdAsync(Guid userId, Guid drinkId)
    {
        return await _context.DrinkRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.DrinkId == drinkId);
    }

    public async Task<double> GetAverageRatingAsync(Guid drinkId)
    {
        var ratings = await _context.DrinkRatings
            .Where(r => r.DrinkId == drinkId)
            .ToListAsync();

        if (!ratings.Any()) return 0;
        return ratings.Average(r => r.Rating);
    }

    public async Task<int> GetRatingCountAsync(Guid drinkId)
    {
        return await _context.DrinkRatings
            .CountAsync(r => r.DrinkId == drinkId);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid drinkId)
    {
        var ratings = await _context.DrinkRatings
            .Where(r => r.DrinkId == drinkId)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        var distribution = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
        };

        foreach (var item in ratings)
        {
            distribution[item.Rating] = item.Count;
        }

        return distribution;
    }

    public async Task<DrinkRating> CreateAsync(DrinkRating rating)
    {
        _context.DrinkRatings.Add(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<DrinkRating> UpdateAsync(DrinkRating rating)
    {
        _context.DrinkRatings.Update(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rating = await _context.DrinkRatings.FindAsync(id);
        if (rating == null) return false;

        _context.DrinkRatings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }

    // ... other methods
}
```

### DTOs

**`DTOs/Drinks/CreateDrinkRatingRequest.cs`**
```csharp
namespace RoundsApp.DTOs.Drinks;

public class CreateDrinkRatingRequest
{
    [Required]
    public Guid DrinkId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Review { get; set; }
}
```

**`DTOs/Drinks/UpdateDrinkRatingRequest.cs`**
```csharp
namespace RoundsApp.DTOs.Drinks;

public class UpdateDrinkRatingRequest
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }

    [MaxLength(2000)]
    public string? Review { get; set; }
}
```

**`DTOs/Drinks/DrinkRatingResponse.cs`**
```csharp
namespace RoundsApp.DTOs.Drinks;

public class DrinkRatingResponse
{
    public Guid Id { get; set; }
    public Guid DrinkId { get; set; }
    public Guid UserId { get; set; }
    public UserResponse? User { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**`DTOs/Drinks/DrinkRatingSummaryResponse.cs`**
```csharp
namespace RoundsApp.DTOs.Drinks;

public class DrinkRatingSummaryResponse
{
    public Guid DrinkId { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}
```

### Endpoints

**`Endpoints/DrinkRatingEndpoints.cs`**
```csharp
namespace RoundsApp.Endpoints;

public static class DrinkRatingEndpoints
{
    public static void MapDrinkRatingEndpoints(this IEndpointRouteBuilder app)
    {
        var ratingApi = app.MapGroup("/api/drink-ratings")
            .WithTags("Drink Ratings")
            .RequireAuthorization();

        ratingApi.MapGet("/drink/{drinkId:guid}", GetRatingsByDrink);
        ratingApi.MapGet("/drink/{drinkId:guid}/summary", GetRatingSummary);
        ratingApi.MapGet("/me", GetMyRatings);
        ratingApi.MapGet("/{id:guid}", GetRatingById);
        ratingApi.MapPost("/", CreateRating);
        ratingApi.MapPut("/{id:guid}", UpdateRating);
        ratingApi.MapDelete("/{id:guid}", DeleteRating);
    }

    private static async Task<IResult> GetRatingsByDrink(
        Guid drinkId,
        IDrinkRatingRepository ratingRepository,
        IDrinkRepository drinkRepository)
    {
        var drink = await drinkRepository.GetByIdAsync(drinkId);
        if (drink == null) return Results.NotFound();

        var ratings = await ratingRepository.GetByDrinkIdAsync(drinkId);
        return Results.Ok(ratings.Select(ToResponse));
    }

    private static async Task<IResult> GetRatingSummary(
        Guid drinkId,
        IDrinkRatingRepository ratingRepository,
        IDrinkRepository drinkRepository)
    {
        var drink = await drinkRepository.GetByIdAsync(drinkId);
        if (drink == null) return Results.NotFound();

        var average = await ratingRepository.GetAverageRatingAsync(drinkId);
        var count = await ratingRepository.GetRatingCountAsync(drinkId);
        var distribution = await ratingRepository.GetRatingDistributionAsync(drinkId);

        return Results.Ok(new DrinkRatingSummaryResponse
        {
            DrinkId = drinkId,
            AverageRating = Math.Round(average, 2),
            TotalRatings = count,
            RatingDistribution = distribution
        });
    }

    private static async Task<IResult> GetMyRatings(
        ClaimsPrincipal user,
        IDrinkRatingRepository ratingRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var ratings = await ratingRepository.GetByUserIdAsync(currentUser.Id);
        return Results.Ok(ratings.Select(ToResponse));
    }

    private static async Task<IResult> CreateRating(
        CreateDrinkRatingRequest request,
        ClaimsPrincipal user,
        IDrinkRatingRepository ratingRepository,
        IDrinkRepository drinkRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var drink = await drinkRepository.GetByIdAsync(request.DrinkId);
        if (drink == null) return Results.NotFound(new { error = "Drink not found" });

        // Check if user already rated this drink
        var existingRating = await ratingRepository.GetByUserAndDrinkIdAsync(
            currentUser.Id, request.DrinkId);
        if (existingRating != null)
        {
            return Results.Conflict(new {
                error = "You have already rated this drink",
                existingRatingId = existingRating.Id
            });
        }

        var rating = new DrinkRating
        {
            Id = Guid.NewGuid(),
            DrinkId = request.DrinkId,
            UserId = currentUser.Id,
            Rating = request.Rating,
            Review = request.Review,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        var created = await ratingRepository.CreateAsync(rating);
        return Results.Created($"/api/drink-ratings/{created.Id}", ToResponse(created));
    }

    private static async Task<IResult> UpdateRating(
        Guid id,
        UpdateDrinkRatingRequest request,
        ClaimsPrincipal user,
        IDrinkRatingRepository ratingRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var rating = await ratingRepository.GetByIdAsync(id);
        if (rating == null) return Results.NotFound();
        if (rating.UserId != currentUser.Id) return Results.Forbid();

        if (request.Rating.HasValue)
        {
            rating.Rating = request.Rating.Value;
        }
        if (request.Review != null)
        {
            rating.Review = request.Review;
        }
        rating.UpdatedById = currentUser.Id;
        rating.UpdatedAt = DateTime.UtcNow;

        var updated = await ratingRepository.UpdateAsync(rating);
        return Results.Ok(ToResponse(updated));
    }

    private static async Task<IResult> DeleteRating(
        Guid id,
        ClaimsPrincipal user,
        IDrinkRatingRepository ratingRepository,
        UserManager<ApplicationUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null) return Results.Unauthorized();

        var rating = await ratingRepository.GetByIdAsync(id);
        if (rating == null) return Results.NotFound();
        if (rating.UserId != currentUser.Id) return Results.Forbid();

        await ratingRepository.DeleteAsync(id);
        return Results.NoContent();
    }

    private static DrinkRatingResponse ToResponse(DrinkRating rating)
    {
        return new DrinkRatingResponse
        {
            Id = rating.Id,
            DrinkId = rating.DrinkId,
            UserId = rating.UserId,
            User = rating.User != null ? new UserResponse
            {
                Id = rating.User.Id,
                UserName = rating.User.UserName
            } : null,
            Rating = rating.Rating,
            Review = rating.Review,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt
        };
    }
}
```

## Modifications to Existing Files

### Models/Drink.cs
Add navigation property:
```csharp
public ICollection<DrinkRating> Ratings { get; set; } = new List<DrinkRating>();
```

### DTOs/Drinks/DrinkResponse.cs
Add rating summary fields:
```csharp
public double? AverageRating { get; set; }
public int RatingCount { get; set; }
```

### DrinkEndpoints.cs
Modify `ToResponse` to include rating info:
```csharp
private static async Task<DrinkResponse> ToResponseWithRating(
    Drink drink,
    IDrinkRatingRepository ratingRepository)
{
    var response = ToResponse(drink);
    response.AverageRating = await ratingRepository.GetAverageRatingAsync(drink.Id);
    response.RatingCount = await ratingRepository.GetRatingCountAsync(drink.Id);
    return response;
}
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/drink-ratings/drink/{drinkId}` | Get all ratings for a drink |
| GET | `/api/drink-ratings/drink/{drinkId}/summary` | Get rating summary (average, count, distribution) |
| GET | `/api/drink-ratings/me` | Get current user's ratings |
| GET | `/api/drink-ratings/{id}` | Get specific rating |
| POST | `/api/drink-ratings` | Create rating (1 per user per drink) |
| PUT | `/api/drink-ratings/{id}` | Update own rating |
| DELETE | `/api/drink-ratings/{id}` | Delete own rating |

## Database Migration

```sql
CREATE TABLE "DrinkRatings" (
    "Id" uuid PRIMARY KEY,
    "DrinkId" uuid NOT NULL REFERENCES "Drinks"("Id") ON DELETE CASCADE,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    "Rating" integer NOT NULL CHECK ("Rating" >= 1 AND "Rating" <= 5),
    "Review" varchar(2000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedById" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    "UpdatedAt" timestamp with time zone,
    "UpdatedById" uuid REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT
);

-- Unique constraint: one rating per user per drink
CREATE UNIQUE INDEX "IX_DrinkRatings_UserId_DrinkId" ON "DrinkRatings"("UserId", "DrinkId");
CREATE INDEX "IX_DrinkRatings_DrinkId" ON "DrinkRatings"("DrinkId");
CREATE INDEX "IX_DrinkRatings_UserId" ON "DrinkRatings"("UserId");
```

## DbContext Configuration

```csharp
builder.Entity<DrinkRating>(entity =>
{
    entity.HasIndex(r => new { r.UserId, r.DrinkId }).IsUnique();

    entity.HasOne(r => r.Drink)
        .WithMany(d => d.Ratings)
        .HasForeignKey(r => r.DrinkId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(r => r.CreatedBy)
        .WithMany()
        .HasForeignKey(r => r.CreatedById)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(r => r.UpdatedBy)
        .WithMany()
        .HasForeignKey(r => r.UpdatedById)
        .OnDelete(DeleteBehavior.Restrict);
});
```

## Response Examples

**Rating Summary:**
```json
{
  "drinkId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "averageRating": 4.25,
  "totalRatings": 48,
  "ratingDistribution": {
    "1": 2,
    "2": 3,
    "3": 8,
    "4": 15,
    "5": 20
  }
}
```

## Validation Rules
1. Rating must be 1-5
2. One rating per user per drink (unique constraint)
3. Only the rating creator can update/delete
4. Review is optional (max 2000 chars)

## Testing
1. Create rating - verify created
2. Create duplicate rating - verify 409 Conflict
3. Get ratings by drink - verify list returned
4. Get summary - verify average calculation
5. Update own rating - verify updated
6. Update other's rating - verify 403 Forbidden
7. Delete rating - verify removed
8. Delete drink - verify cascade deletes ratings
