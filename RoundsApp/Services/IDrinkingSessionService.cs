// <copyright file="IDrinkingSessionService.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using RoundsApp.DTOs;
using RoundsApp.Models;

namespace RoundsApp.Services;

public interface IDrinkingSessionService
{
    Task<DrinkingSession> CreateDrinkingSessionAsync(CreateDrinkingSessionRequest session, Guid creatorId);
    Task<DrinkingSession> UpdateDrinkingSessionAsync(Guid sessionId, CreateDrinkingSessionRequest session, Guid userId);
    Task<bool> DeleteDrinkingSessionAsync(Guid sessionId, Guid userId);
    Task<DrinkingSession?> GetDrinkingSessionByIdAsync(Guid sessionId);
    Task<List<DrinkingSessionParticipation>> GetParticipantsAsync(Guid sessionId);
    Task<bool> AddParticipantAsync(Guid sessionId, Guid userId, Guid addedById);
    Task<bool> RemoveParticipantAsync(Guid sessionId, Guid participantId, Guid userId);
    Task<List<DrinkingSessionImage>> GetImagesAsync(Guid sessionId);
    Task<bool> AddImageAsync(Guid sessionId, byte[] imageData, Guid addedById);
    Task<bool> DeleteImageAsync(Guid imageId, Guid userId);
    Task<bool> RecordDrinkAsync(Guid sessionId, Guid participantId, Guid drinkId, Guid recordedById);
    Task<bool> RemoveDrinkAsync(Guid sessionParticipationDrinkId, Guid userId);
    Task<List<DrinkingSessionParticipationDrink>> GetParticipantDrinksAsync(Guid sessionId, Guid participantId);
    Task<List<DrinkingSession>> GetDrinkingSessionsByParticipantsIdAsync(Guid userId);
    Task<List<DrinkingSessionParticipationDrink>> GetAllDrinksInSessionAsync(Guid sessionId);
    Task<int> GetTotalDrinkCountInSessionAsync(Guid sessionId);
    Task<Dictionary<Guid, int>> GetDrinkCountsPerParticipantAsync(Guid sessionId);
}
