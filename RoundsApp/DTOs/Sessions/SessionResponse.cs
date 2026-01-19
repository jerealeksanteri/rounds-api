// <copyright file="SessionResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Users;

namespace RoundsApp.DTOs.Sessions;

public class SessionResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public Guid? LocationId { get; set; }

    public SessionLocationResponse? Location { get; set; }

    public Guid CreatedById { get; set; }

    public UserResponse? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<ParticipantResponse> Participants { get; set; } = new();

    public List<SessionInviteResponse> Invites { get; set; } = new();

    public List<CommentResponse> Comments { get; set; } = new();

    public List<SessionImageResponse> Images { get; set; } = new();

    public List<SessionTagResponse> Tags { get; set; } = new();

    public List<SessionAchievementResponse> Achievements { get; set; } = new();

    public List<UserDrinkResponse> Drinks { get; set; } = new();
}
