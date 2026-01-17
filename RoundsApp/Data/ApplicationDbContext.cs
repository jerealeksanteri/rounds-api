// <copyright file="ApplicationDbContext.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoundsApp.Models;

namespace RoundsApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity table names if needed
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable(name: "Users");
        });
    }
}
