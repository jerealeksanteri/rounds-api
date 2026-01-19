// <copyright file="UserDrinkResponse.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs.Drinks;
using RoundsApp.DTOs.Users;

namespace RoundsApp.DTOs.Sessions;

public class UserDrinkResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public UserResponse? User { get; set; }

    public Guid DrinkId { get; set; }

    public DrinkResponse? Drink { get; set; }

    public Guid SessionId { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }
}
