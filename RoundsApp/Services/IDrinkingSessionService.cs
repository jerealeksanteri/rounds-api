// <copyright file="IDrinkingSessionService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs;
using RoundsApp.Models;

namespace RoundsApp.Services;

public interface IDrinkingSessionService
{
    Task<DrinkingSession> CreateDrinkingSessionAsync(CreateDrinkingSessionRequest session, Guid creatorId);
    Task<DrinkingSession> UpdateDrinkingSessionAsync(CreateDrinkingSessionRequest session, Guid updatedById);
    Task<DrinkingSession?> GetDrinkingSessionByIdAsync(Guid sessionId);
    Task<List<DrinkingSessionParticipation>> GetParticipantsAsync(Guid sessionId);
    Task<bool> AddParticipantAsync(Guid sessionId, Guid userId, Guid addedById);
    Task<bool> RemoveParticipantAsync(Guid sessionId, Guid userId, Guid removedById);
    Task<List<DrinkingSessionImage>> GetImagesAsync(Guid sessionId);
    Task<bool> AddImageAsync(Guid sessionId, byte[] imageData, Guid addedById);
    Task<bool> DeleteImageAsync(Guid imageId, Guid deletedById);
    Task<bool> RecordDrinkAsync(Guid sessionId, Guid participantId, Guid drinkId, Guid recordedById);
    Task<bool> RemoveDrinkAsync(Guid sessionParticipationDrinkId);
    Task<List<DrinkingSessionParticipationDrink>> GetParticipantDrinksAsync(Guid sessionId, Guid participantId);
    Task<bool> DeleteDrinkingSessionAsync(Guid sessionId);
    Task<List<DrinkingSession>> GetDrinkingSessionsByUserIdAsync(Guid userId);
    Task<List<DrinkingSessionParticipation>> GetDrinkingSessionsByParticipantsIdAsync(Guid userId);

    // DO NOT DDOS THE SERVER
    // Task<List<DrinkingSession>> GetAllDrinkingSessionsAsync();
    // NO NEED WE HAVE UPDATE METHOD
    // Task<bool> UpdateScheduledAtAsync(Guid sessionId, DateTime? scheduledAt, Guid updatedById);
    Task<List<DrinkingSessionParticipationDrink>> GetAllDrinksInSessionAsync(Guid sessionId);
    Task<int> GetTotalDrinkCountInSessionAsync(Guid sessionId);
    Task<Dictionary<Guid, int>> GetDrinkCountsPerParticipantAsync(Guid sessionId);
}
