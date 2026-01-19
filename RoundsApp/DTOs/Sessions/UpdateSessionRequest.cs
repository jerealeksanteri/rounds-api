// <copyright file="UpdateSessionRequest.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

namespace RoundsApp.DTOs.Sessions;

public class UpdateSessionRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public Guid? LocationId { get; set; }
}
