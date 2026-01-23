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

    public DbSet<DrinkingSession> DrinkingSessions { get; set; } = null!;

    public DbSet<SessionLocation> SessionLocations { get; set; } = null!;

    public DbSet<SessionParticipant> SessionParticipants { get; set; } = null!;

    public DbSet<SessionInvite> SessionInvites { get; set; } = null!;

    public DbSet<SessionComment> SessionComments { get; set; } = null!;

    public DbSet<SessionImage> SessionImages { get; set; } = null!;

    public DbSet<SessionTag> SessionTags { get; set; } = null!;

    public DbSet<DrinkType> DrinkTypes { get; set; } = null!;

    public DbSet<Drink> Drinks { get; set; } = null!;

    public DbSet<DrinkImage> DrinkImages { get; set; } = null!;

    public DbSet<UserDrink> UserDrinks { get; set; } = null!;

    public DbSet<UserFavouriteDrink> UserFavouriteDrinks { get; set; } = null!;

    public DbSet<Achievement> Achievements { get; set; } = null!;

    public DbSet<UserAchievement> UserAchievements { get; set; } = null!;

    public DbSet<SessionAchievement> SessionAchievements { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;

    public DbSet<Friendship> Friendships { get; set; } = null!;

    public DbSet<FriendGroup> FriendGroups { get; set; } = null!;

    public DbSet<FriendGroupMember> FriendGroupMembers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity table names if needed
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable(name: "Users");
        });

        // Configure Friendship composite primary key
        builder.Entity<Friendship>(entity =>
        {
            entity.HasKey(f => new { f.UserId, f.FriendId });

            // Configure relationships to avoid cascade delete cycles
            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.UpdatedBy)
                .WithMany()
                .HasForeignKey(f => f.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure cascade delete behavior for other entities to avoid cycles
        builder.Entity<DrinkingSession>(entity =>
        {
            entity.HasOne(ds => ds.CreatedBy)
                .WithMany()
                .HasForeignKey(ds => ds.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ds => ds.UpdatedBy)
                .WithMany()
                .HasForeignKey(ds => ds.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SessionParticipant>(entity =>
        {
            entity.HasOne(sp => sp.CreatedBy)
                .WithMany()
                .HasForeignKey(sp => sp.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sp => sp.UpdatedBy)
                .WithMany()
                .HasForeignKey(sp => sp.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SessionInvite>(entity =>
        {
            entity.HasOne(si => si.CreatedBy)
                .WithMany()
                .HasForeignKey(si => si.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(si => si.UpdatedBy)
                .WithMany()
                .HasForeignKey(si => si.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(si => si.User)
                .WithMany()
                .HasForeignKey(si => si.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SessionComment>(entity =>
        {
            entity.HasOne(sc => sc.CreatedBy)
                .WithMany()
                .HasForeignKey(sc => sc.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sc => sc.UpdatedBy)
                .WithMany()
                .HasForeignKey(sc => sc.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SessionImage>(entity =>
        {
            entity.HasOne(si => si.CreatedBy)
                .WithMany()
                .HasForeignKey(si => si.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(si => si.UpdatedBy)
                .WithMany()
                .HasForeignKey(si => si.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SessionLocation>(entity =>
        {
            entity.HasOne(sl => sl.CreatedBy)
                .WithMany()
                .HasForeignKey(sl => sl.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sl => sl.UpdatedBy)
                .WithMany()
                .HasForeignKey(sl => sl.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SessionTag>(entity =>
        {
            entity.HasOne(st => st.CreatedBy)
                .WithMany()
                .HasForeignKey(st => st.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DrinkType>(entity =>
        {
            entity.HasOne(dt => dt.CreatedBy)
                .WithMany()
                .HasForeignKey(dt => dt.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dt => dt.UpdatedBy)
                .WithMany()
                .HasForeignKey(dt => dt.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Drink>(entity =>
        {
            entity.HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedBy)
                .WithMany()
                .HasForeignKey(d => d.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DrinkImage>(entity =>
        {
            entity.HasOne(di => di.CreatedBy)
                .WithMany()
                .HasForeignKey(di => di.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(di => di.UpdatedBy)
                .WithMany()
                .HasForeignKey(di => di.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserDrink>(entity =>
        {
            entity.HasOne(ud => ud.CreatedBy)
                .WithMany()
                .HasForeignKey(ud => ud.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ud => ud.UpdatedBy)
                .WithMany()
                .HasForeignKey(ud => ud.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ud => ud.User)
                .WithMany()
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserFavouriteDrink>(entity =>
        {
            entity.HasOne(ufd => ufd.CreatedBy)
                .WithMany()
                .HasForeignKey(ufd => ufd.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ufd => ufd.User)
                .WithMany()
                .HasForeignKey(ufd => ufd.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Achievement>(entity =>
        {
            entity.HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.UpdatedBy)
                .WithMany()
                .HasForeignKey(a => a.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserAchievement>(entity =>
        {
            entity.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FriendGroup>(entity =>
        {
            entity.HasOne(fg => fg.Owner)
                .WithMany()
                .HasForeignKey(fg => fg.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.UpdatedBy)
                .WithMany()
                .HasForeignKey(a => a.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
