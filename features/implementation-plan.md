# Implementation Plan: 6 New Features for RoundsApp

## Feature Specifications

Detailed specifications for each feature are in this directory:

| # | Feature | File | Dependencies |
|---|---------|------|--------------|
| 1 | Real-Time Notifications (SignalR) | `real-time-notifications.md` | None (foundation) |
| 2 | Friend Groups | `friend-groups.md` | SignalR (for bulk invite notifications) |
| 3 | User Mentions | `user-mentions.md` | SignalR |
| 4 | Activity Feed | `activity-feed.md` | Friendships |
| 5 | Drink Ratings & Reviews | `drink-ratings.md` | None |
| 6 | Achievement Sharing | `achievement-sharing.md` | UserAchievement |

## Implementation Order

1. **Real-Time Notifications** - Foundation for push notifications
2. **Friend Groups** - Enables bulk session invites
3. **User Mentions** - Uses notification service
4. **Activity Feed** - Uses friendships for feed filtering
5. **Drink Ratings** - Independent feature
6. **Achievement Sharing** - Independent feature

## Summary of Changes

### New Files (45 total)

| Category | Count | Details |
|----------|-------|---------|
| Hubs | 1 | `NotificationHub` |
| Models | 6 | `ActivityFeedItem`, `AchievementShare`, `CommentMention`, `DrinkRating`, `FriendGroup`, `FriendGroupMember` |
| Repository Interfaces | 6 | One per model |
| Repository Implementations | 6 | One per model |
| Service Interfaces | 5 | `INotificationService`, `IActivityFeedService`, `IMentionService`, `IDeepLinkService`, `IFriendGroupValidationService` |
| Service Implementations | 5 | One per interface |
| Endpoints | 4 | `ActivityFeedEndpoints`, `AchievementShareEndpoints`, `DrinkRatingEndpoints`, `FriendGroupEndpoints` |
| DTOs | 16 | Request/Response objects for each feature |

### Files to Modify

| File | Changes |
|------|---------|
| `Program.cs` | Add SignalR, register new services and repositories, map new endpoints |
| `ApplicationDbContext.cs` | Add 6 new DbSets, configure entity relationships |
| `SessionCommentEndpoints.cs` | Add mention parsing and notifications |
| `SessionInviteEndpoints.cs` | Add real-time notifications, bulk invite endpoint |
| `FriendshipEndpoints.cs` | Add real-time notifications on friend request |
| `SessionEndpoints.cs` | Record activity feed on session create |
| `SessionParticipantEndpoints.cs` | Record activity feed on session join |
| `Models/Drink.cs` | Add `Ratings` navigation property |
| `Models/SessionComment.cs` | Add `Mentions` navigation property |
| `DTOs/Drinks/DrinkResponse.cs` | Add `AverageRating`, `RatingCount` fields |
| `DTOs/Sessions/CommentResponse.cs` | Add `Mentions` list |

### Database Tables (6 new)

```
AchievementShares
├── Id (PK)
├── UserAchievementId (FK)
├── SharedByUserId (FK)
├── ShareToken (unique)
├── Platform, ViewCount, CreatedAt, ExpiresAt

ActivityFeedItems
├── Id (PK)
├── UserId (FK)
├── ActivityType, RelatedEntityId
├── Metadata (JSONB)
├── IsPublic, CreatedAt

CommentMentions
├── Id (PK)
├── CommentId (FK, cascade delete)
├── MentionedUserId (FK)
├── StartPosition, Length, CreatedAt

DrinkRatings
├── Id (PK)
├── DrinkId (FK, cascade delete)
├── UserId (FK, unique with DrinkId)
├── Rating (1-5), Review
├── CreatedAt, CreatedById, UpdatedAt, UpdatedById

FriendGroups
├── Id (PK)
├── OwnerId (FK)
├── Name, Description
├── CreatedAt, CreatedById, UpdatedAt, UpdatedById

FriendGroupMembers
├── GroupId + UserId (composite PK)
├── AddedAt, AddedById
```

## API Endpoints Summary

### Real-Time Notifications
- `WebSocket /hubs/notifications` - SignalR hub connection

### Friend Groups
```
GET    /api/friend-groups              - Get my groups
GET    /api/friend-groups/{id}         - Get group with members
POST   /api/friend-groups              - Create group
PUT    /api/friend-groups/{id}         - Update group
DELETE /api/friend-groups/{id}         - Delete group
POST   /api/friend-groups/{id}/members - Add members
DELETE /api/friend-groups/{id}/members/{userId} - Remove member
POST   /api/friend-groups/{id}/invite-to-session - Bulk invite
```

### Activity Feed
```
GET    /api/activity-feed              - Friends' activities
GET    /api/activity-feed/me           - Own activities
GET    /api/activity-feed/user/{id}    - User's public activities
DELETE /api/activity-feed/{id}         - Delete own activity
```

### Drink Ratings
```
GET    /api/drink-ratings/drink/{id}         - Ratings for drink
GET    /api/drink-ratings/drink/{id}/summary - Average & distribution
GET    /api/drink-ratings/me                 - My ratings
POST   /api/drink-ratings                    - Create rating
PUT    /api/drink-ratings/{id}               - Update rating
DELETE /api/drink-ratings/{id}               - Delete rating
```

### Achievement Sharing
```
POST   /api/achievement-shares              - Create share link
GET    /api/achievement-shares/view/{token} - View shared (public)
GET    /api/achievement-shares/me           - My shares
DELETE /api/achievement-shares/{id}         - Delete share
```

## Verification Steps

### After Each Feature
1. Run `dotnet build` - ensure no compilation errors
2. Run `dotnet test` - ensure existing tests pass
3. Create integration tests for new endpoints

### End-to-End Testing
1. **SignalR**: Connect via WebSocket, verify notifications push in real-time
2. **Friend Groups**: Create group, add members, bulk invite to session
3. **Mentions**: Post comment with @username, verify notification received
4. **Activity Feed**: Join session, verify activity appears in friends' feed
5. **Drink Ratings**: Rate drink, verify average calculation
6. **Achievement Sharing**: Generate link, open in browser, verify public view

## Migration Command

After implementing all models:
```bash
dotnet ef migrations add AddNewFeatures
dotnet ef database update
```