// <copyright file="ApplicationDbContext.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoundsApp.Models;

namespace RoundsApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DrinkingSession> DrinkingSessions { get; set; }
    public DbSet<DrinkingSessionParticipation> DrinkingSessionParticipations { get; set; }
    public DbSet<DrinkingSessionImage> DrinkingSessionImages { get; set; }
    public DbSet<DrinkingSessionParticipationDrink> DrinkingSessionParticipationDrinks { get; set; }
    public DbSet<Drink> Drinks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity table names if needed
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable(name: "Users");
        });

        builder.Entity<DrinkingSession>(entity =>
        {
            entity.ToTable(name: "DrinkingSessions");
        });

        builder.Entity<DrinkingSessionParticipation>(entity =>
        {
            entity.ToTable(name: "DrinkingSessionParticipations");
        });

        builder.Entity<DrinkingSessionImage>(entity =>
        {
            entity.ToTable(name: "DrinkingSessionImages");
        });

        builder.Entity<DrinkingSessionParticipationDrink>(entity =>
        {
            entity.ToTable(name: "DrinkingSessionParticipationDrinks");
        });
    }
}
