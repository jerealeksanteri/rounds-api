// <copyright file="DrinkingSessionEndpointsTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using RoundsApp.DTOs;
using RoundsApp.Endpoints;
using RoundsApp.Models;
using RoundsApp.Services;
using Xunit;

namespace RoundsApp.Tests;

public class DrinkingSessionEndpointsTests
{
    private readonly Mock<IDrinkingSessionService> mockService;
    private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
    private readonly Guid testUserId = Guid.NewGuid();

    public DrinkingSessionEndpointsTests()
    {
        this.mockService = new Mock<IDrinkingSessionService>();
        this.mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        // Setup default HttpContext with authenticated user
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, this.testUserId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;

        this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    [Fact]
    public async Task CreateDrinkingSession_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new CreateDrinkingSessionRequest
        {
            Name = "Test Session",
            Description = "Test Description",
            ScheduledAt = DateTime.UtcNow.AddDays(1),
        };

        var expectedSession = new DrinkingSession
        {
            Id = Guid.NewGuid(),
            Title = request.Name,
            Description = request.Description,
            ScheduledAt = request.ScheduledAt,
            CreatedById = this.testUserId,
        };

        this.mockService
            .Setup(x => x.CreateDrinkingSessionAsync(request, this.testUserId))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await this.InvokeCreateDrinkingSession(request);

        // Assert
        var okResult = result as Ok<DrinkingSession>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedSession);
        this.mockService.Verify(x => x.CreateDrinkingSessionAsync(request, this.testUserId), Times.Once);
    }

    [Fact]
    public async Task CreateDrinkingSession_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateDrinkingSessionRequest { Name = "Test" };
        var httpContext = new DefaultHttpContext();
        this.mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await this.InvokeCreateDrinkingSession(request);

        // Assert
        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task UpdateDrinkingSession_AsCreator_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new CreateDrinkingSessionRequest
        {
            Name = "Updated Session",
            Description = "Updated Description",
        };

        var updatedSession = new DrinkingSession
        {
            Id = sessionId,
            Title = request.Name,
            Description = request.Description,
            CreatedById = this.testUserId,
        };

        this.mockService
            .Setup(x => x.UpdateDrinkingSessionAsync(sessionId, request, this.testUserId))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await this.InvokeUpdateDrinkingSession(sessionId, request);

        // Assert
        var okResult = result as Ok<DrinkingSession>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(updatedSession);
    }

    [Fact]
    public async Task UpdateDrinkingSession_AsNonCreator_ReturnsForbidden()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new CreateDrinkingSessionRequest { Name = "Updated" };

        this.mockService
            .Setup(x => x.UpdateDrinkingSessionAsync(sessionId, request, this.testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("Only the creator can update"));

        // Act
        var result = await this.InvokeUpdateDrinkingSession(sessionId, request);

        // Assert
        var problemResult = result as ProblemHttpResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateDrinkingSession_WithNonExistentSession_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new CreateDrinkingSessionRequest { Name = "Updated" };

        this.mockService
            .Setup(x => x.UpdateDrinkingSessionAsync(sessionId, request, this.testUserId))
            .ThrowsAsync(new InvalidOperationException("Session not found"));

        // Act
        var result = await this.InvokeUpdateDrinkingSession(sessionId, request);

        // Assert
        var notFoundResult = result as NotFound<string>;
        notFoundResult.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteDrinkingSession_AsCreator_ReturnsNoContent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.DeleteDrinkingSessionAsync(sessionId, this.testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await this.InvokeDeleteDrinkingSession(sessionId);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task DeleteDrinkingSession_WithNonExistentSession_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.DeleteDrinkingSessionAsync(sessionId, this.testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await this.InvokeDeleteDrinkingSession(sessionId);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task DeleteDrinkingSession_AsNonCreator_ReturnsForbidden()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.DeleteDrinkingSessionAsync(sessionId, this.testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("Only the creator can delete"));

        // Act
        var result = await this.InvokeDeleteDrinkingSession(sessionId);

        // Assert
        var problemResult = result as ProblemHttpResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task AddParticipant_WithValidSession_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.AddParticipantAsync(sessionId, this.testUserId, this.testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await this.InvokeAddParticipant(sessionId);

        // Assert
        result.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task AddParticipant_WithNonExistentSession_ReturnsBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.AddParticipantAsync(sessionId, this.testUserId, this.testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await this.InvokeAddParticipant(sessionId);

        // Assert
        var badRequestResult = result as BadRequest<string>;
        badRequestResult.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveParticipant_AsCreator_ReturnsNoContent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.RemoveParticipantAsync(sessionId, participantId, this.testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await this.InvokeRemoveParticipant(sessionId, participantId);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task RemoveParticipant_AsNonCreator_ReturnsForbidden()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.RemoveParticipantAsync(sessionId, participantId, this.testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("Only the creator can remove participants"));

        // Act
        var result = await this.InvokeRemoveParticipant(sessionId, participantId);

        // Assert
        var problemResult = result as ProblemHttpResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task RemoveParticipant_WithNonExistentParticipant_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.RemoveParticipantAsync(sessionId, participantId, this.testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await this.InvokeRemoveParticipant(sessionId, participantId);

        // Assert
        var notFoundResult = result as NotFound<string>;
        notFoundResult.Should().NotBeNull();
    }

    [Fact]
    public async Task AddImage_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new AddImageRequest { ImageData = new byte[] { 1, 2, 3 } };
        this.mockService
            .Setup(x => x.AddImageAsync(sessionId, request.ImageData, this.testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await this.InvokeAddImage(sessionId, request);

        // Assert
        result.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task AddImage_WithNonExistentSession_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new AddImageRequest { ImageData = new byte[] { 1, 2, 3 } };
        this.mockService
            .Setup(x => x.AddImageAsync(sessionId, request.ImageData, this.testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await this.InvokeAddImage(sessionId, request);

        // Assert
        var notFoundResult = result as NotFound<string>;
        notFoundResult.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveImage_AsCreator_ReturnsNoContent()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.DeleteImageAsync(imageId, this.testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await this.InvokeRemoveImage(imageId);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task RemoveImage_AsNonCreator_ReturnsForbidden()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.DeleteImageAsync(imageId, this.testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("Only the session creator can delete images"));

        // Act
        var result = await this.InvokeRemoveImage(imageId);

        // Assert
        var problemResult = result as ProblemHttpResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task RecordDrink_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new RecordDrinkRequest
        {
            ParticipantId = Guid.NewGuid(),
            DrinkId = Guid.NewGuid(),
        };

        this.mockService
            .Setup(x => x.RecordDrinkAsync(sessionId, request.ParticipantId, request.DrinkId, this.testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await this.InvokeRecordDrink(sessionId, request);

        // Assert
        result.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task RecordDrink_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new RecordDrinkRequest
        {
            ParticipantId = Guid.NewGuid(),
            DrinkId = Guid.NewGuid(),
        };

        this.mockService
            .Setup(x => x.RecordDrinkAsync(sessionId, request.ParticipantId, request.DrinkId, this.testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await this.InvokeRecordDrink(sessionId, request);

        // Assert
        var badRequestResult = result as BadRequest<string>;
        badRequestResult.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveDrink_AsCreator_ReturnsNoContent()
    {
        // Arrange
        var drinkId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.RemoveDrinkAsync(drinkId, this.testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await this.InvokeRemoveDrink(drinkId);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task RemoveDrink_AsNonCreator_ReturnsForbidden()
    {
        // Arrange
        var drinkId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.RemoveDrinkAsync(drinkId, this.testUserId))
            .ThrowsAsync(new UnauthorizedAccessException("Only the session creator can remove drinks"));

        // Act
        var result = await this.InvokeRemoveDrink(drinkId);

        // Assert
        var problemResult = result as ProblemHttpResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task RemoveDrink_WithNonExistentDrink_ReturnsNotFound()
    {
        // Arrange
        var drinkId = Guid.NewGuid();
        this.mockService
            .Setup(x => x.RemoveDrinkAsync(drinkId, this.testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await this.InvokeRemoveDrink(drinkId);

        // Assert
        var notFoundResult = result as NotFound<string>;
        notFoundResult.Should().NotBeNull();
    }

    private async Task<IResult> InvokeCreateDrinkingSession(CreateDrinkingSessionRequest request)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "CreateDrinkingSession",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { request, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeUpdateDrinkingSession(Guid sessionId, CreateDrinkingSessionRequest request)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "UpdateDrinkingSession",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { sessionId, request, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeDeleteDrinkingSession(Guid sessionId)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "DeleteDrinkingSession",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { sessionId, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeAddParticipant(Guid sessionId)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "AddParticipant",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { sessionId, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeRemoveParticipant(Guid sessionId, Guid participantId)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "RemoveParticipant",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { sessionId, participantId, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeAddImage(Guid sessionId, AddImageRequest request)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "AddImage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { sessionId, request, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeRemoveImage(Guid imageId)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "RemoveImage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { imageId, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeRecordDrink(Guid sessionId, RecordDrinkRequest request)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "RecordDrink",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { sessionId, request, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }

    private async Task<IResult> InvokeRemoveDrink(Guid drinkId)
    {
        var method = typeof(DrinkingSessionEndpoints).GetMethod(
            "RemoveDrink",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { drinkId, this.mockService.Object, this.mockHttpContextAccessor.Object });
        return await (Task<IResult>)result!;
    }
}
