// <copyright file="DrinkingSessionService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using RoundsApp.Data;
using RoundsApp.DTOs;
using RoundsApp.Models;

namespace RoundsApp.Services;

public class DrinkingSessionService : IDrinkingSessionService
{
    private readonly ApplicationDbContext context;

    public DrinkingSessionService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<DrinkingSession> CreateDrinkingSessionAsync(CreateDrinkingSessionRequest session, Guid creatorId)
    {
        var drinkingSession = new DrinkingSession
        {
            Title = session.Name,
            Description = session.Description,
            ScheduledAt = session.ScheduledAt,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow,
        };

        this.context.DrinkingSessions.Add(drinkingSession);
        await this.context.SaveChangesAsync();

        return drinkingSession;
    }

    public async Task<DrinkingSession> UpdateDrinkingSessionAsync(Guid sessionId, CreateDrinkingSessionRequest session, Guid updatedById)
    {
        var existingSession = await this.context.DrinkingSessions.FindAsync(sessionId);
        if (existingSession == null)
        {
            throw new InvalidOperationException($"Drinking session with ID {sessionId} not found");
        }

        existingSession.Title = session.Name;
        existingSession.Description = session.Description;
        existingSession.ScheduledAt = session.ScheduledAt;
        existingSession.UpdatedById = updatedById;
        existingSession.UpdatedAt = DateTime.UtcNow;

        await this.context.SaveChangesAsync();

        return existingSession;
    }

    public async Task<DrinkingSession?> GetDrinkingSessionByIdAsync(Guid sessionId)
    {
        return await this.context.DrinkingSessions
            .Include(s => s.Participants)
            .Include(s => s.Images)
            .Include(s => s.CreatedBy)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<List<DrinkingSessionParticipation>> GetParticipantsAsync(Guid sessionId)
    {
        return await this.context.DrinkingSessionParticipations
            .Where(p => p.DrinkingSessionId == sessionId)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<bool> AddParticipantAsync(Guid sessionId, Guid userId, Guid addedById)
    {
        var session = await this.context.DrinkingSessions.FindAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        var existingParticipation = await this.context.DrinkingSessionParticipations
            .FirstOrDefaultAsync(p => p.DrinkingSessionId == sessionId && p.UserId == userId);

        if (existingParticipation != null)
        {
            return false; // Already a participant
        }

        var participation = new DrinkingSessionParticipation
        {
            DrinkingSessionId = sessionId,
            UserId = userId,
            CreatedById = addedById,
            CreatedAt = DateTime.UtcNow,
            JoinedAt = DateTime.UtcNow,
        };

        this.context.DrinkingSessionParticipations.Add(participation);
        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveParticipantAsync(Guid sessionId, Guid userId, Guid removedById)
    {
        var participation = await this.context.DrinkingSessionParticipations
            .FirstOrDefaultAsync(p => p.DrinkingSessionId == sessionId && p.UserId == userId);

        if (participation == null)
        {
            return false;
        }

        this.context.DrinkingSessionParticipations.Remove(participation);
        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task<List<DrinkingSessionImage>> GetImagesAsync(Guid sessionId)
    {
        return await this.context.DrinkingSessionImages
            .Where(i => i.DrinkingSessionId == sessionId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> AddImageAsync(Guid sessionId, byte[] imageData, Guid addedById)
    {
        var session = await this.context.DrinkingSessions.FindAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        var image = new DrinkingSessionImage
        {
            DrinkingSessionId = sessionId,
            ImageData = imageData,
            CreatedById = addedById,
            CreatedAt = DateTime.UtcNow,
        };

        this.context.DrinkingSessionImages.Add(image);
        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteImageAsync(Guid imageId, Guid deletedById)
    {
        var image = await this.context.DrinkingSessionImages.FindAsync(imageId);
        if (image == null)
        {
            return false;
        }

        this.context.DrinkingSessionImages.Remove(image);
        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RecordDrinkAsync(Guid sessionId, Guid participantId, Guid drinkId, Guid recordedById)
    {
        var participation = await this.context.DrinkingSessionParticipations
            .FirstOrDefaultAsync(p => p.Id == participantId && p.DrinkingSessionId == sessionId);

        if (participation == null)
        {
            return false;
        }

        var drink = await this.context.Drinks.FindAsync(drinkId);
        if (drink == null)
        {
            return false;
        }

        var participationDrink = new DrinkingSessionParticipationDrink
        {
            DrinkingSessionParticipationId = participantId,
            DrinkId = drinkId,
            CreatedById = recordedById,
            CreatedAt = DateTime.UtcNow,
        };

        this.context.DrinkingSessionParticipationDrinks.Add(participationDrink);
        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveDrinkAsync(Guid sessionParticipationDrinkId)
    {
        var participationDrink = await this.context.DrinkingSessionParticipationDrinks
            .FindAsync(sessionParticipationDrinkId);

        if (participationDrink == null)
        {
            return false;
        }

        this.context.DrinkingSessionParticipationDrinks.Remove(participationDrink);
        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task<List<DrinkingSessionParticipationDrink>> GetParticipantDrinksAsync(Guid sessionId, Guid participantId)
    {
        return await this.context.DrinkingSessionParticipationDrinks
            .Include(d => d.Drink)
            .Include(d => d.DrinkingSessionParticipation)
            .Where(d => d.DrinkingSessionParticipation.DrinkingSessionId == sessionId
                     && d.DrinkingSessionParticipationId == participantId)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteDrinkingSessionAsync(Guid sessionId)
    {
        var session = await this.context.DrinkingSessions.FindAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        this.context.DrinkingSessions.Remove(session);
        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task<List<DrinkingSession>> GetDrinkingSessionsByUserIdAsync(Guid userId)
    {
        return await this.context.DrinkingSessions
            .Where(s => s.CreatedById == userId)
            .Include(s => s.Participants)
            .Include(s => s.Images)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<DrinkingSession>> GetDrinkingSessionsByParticipantsIdAsync(Guid userId)
    {
        var participations = await this.context.DrinkingSessionParticipations
            .Where(p => p.UserId == userId)
            .Include(p => p.DrinkingSession)
                .ThenInclude(s => s.Participants)
            .Include(p => p.DrinkingSession)
                .ThenInclude(s => s.Images)
            .Select(p => p.DrinkingSession)
            .ToListAsync();

        return participations!;
    }

    public async Task<List<DrinkingSessionParticipationDrink>> GetAllDrinksInSessionAsync(Guid sessionId)
    {
        return await this.context.DrinkingSessionParticipationDrinks
            .Include(d => d.Drink)
            .Include(d => d.DrinkingSessionParticipation)
                .ThenInclude(p => p.User)
            .Where(d => d.DrinkingSessionParticipation.DrinkingSessionId == sessionId)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalDrinkCountInSessionAsync(Guid sessionId)
    {
        return await this.context.DrinkingSessionParticipationDrinks
            .Include(d => d.DrinkingSessionParticipation)
            .Where(d => d.DrinkingSessionParticipation.DrinkingSessionId == sessionId)
            .CountAsync();
    }

    public async Task<Dictionary<Guid, int>> GetDrinkCountsPerParticipantAsync(Guid sessionId)
    {
        var drinkCounts = await this.context.DrinkingSessionParticipationDrinks
            .Include(d => d.DrinkingSessionParticipation)
            .Where(d => d.DrinkingSessionParticipation.DrinkingSessionId == sessionId)
            .GroupBy(d => d.DrinkingSessionParticipation.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        return drinkCounts.ToDictionary(x => x.UserId, x => x.Count);
    }
}
