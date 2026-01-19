// <copyright file="UserResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Users;

public class UserResponse
{
    public Guid Id { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }
}
