// <copyright file="ITokenService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.Models;

namespace RoundsApp.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}
