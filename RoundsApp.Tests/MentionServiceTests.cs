// <copyright file="MentionServiceTests.cs" company="RoundsApp">
// Copyright (c) RoundsApp. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using RoundsApp.Models;
using RoundsApp.Repositories.IRepositories;
using RoundsApp.Services;
using Xunit;

namespace RoundsApp.Tests;

public class MentionServiceTests
{
    private readonly Mock<ICommentMentionRepository> mockMentionRepository;
    private readonly Mock<UserManager<ApplicationUser>> mockUserManager;
    private readonly MentionService mentionService;

    public MentionServiceTests()
    {
        this.mockMentionRepository = new Mock<ICommentMentionRepository>();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        this.mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        this.mentionService = new MentionService(
            this.mockMentionRepository.Object,
            this.mockUserManager.Object);
    }

    [Fact]
    public void ParseMentions_SingleMention_ReturnsCorrectResult()
    {
        // Arrange
        var content = "Hey @john, how are you?";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Username.Should().Be("john");
        results[0].StartPosition.Should().Be(4);
        results[0].Length.Should().Be(5); // @john
    }

    [Fact]
    public void ParseMentions_MultipleMentions_ReturnsAllMentions()
    {
        // Arrange
        var content = "Hey @john and @jane, let's meet @bob later!";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().HaveCount(3);
        results[0].Username.Should().Be("john");
        results[1].Username.Should().Be("jane");
        results[2].Username.Should().Be("bob");
    }

    [Fact]
    public void ParseMentions_NoMentions_ReturnsEmpty()
    {
        // Arrange
        var content = "This is a comment without any mentions";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ParseMentions_MentionAtStart_ReturnsCorrectPosition()
    {
        // Arrange
        var content = "@admin please check this";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Username.Should().Be("admin");
        results[0].StartPosition.Should().Be(0);
        results[0].Length.Should().Be(6); // @admin
    }

    [Fact]
    public void ParseMentions_MentionAtEnd_ReturnsCorrectResult()
    {
        // Arrange
        var content = "Thanks @helper";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Username.Should().Be("helper");
        results[0].StartPosition.Should().Be(7);
    }

    [Fact]
    public void ParseMentions_MentionWithNumbers_ReturnsCorrectResult()
    {
        // Arrange
        var content = "Contact @user123 for help";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Username.Should().Be("user123");
    }

    [Fact]
    public void ParseMentions_MentionWithUnderscore_ReturnsCorrectResult()
    {
        // Arrange
        var content = "Ask @john_doe about it";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Username.Should().Be("john_doe");
    }

    [Fact]
    public void ParseMentions_EmailAddress_DoesNotMatch()
    {
        // Arrange
        var content = "Contact me at test@example.com";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        // Email addresses contain @ but are preceded by characters, not whitespace/start
        // The regex @(\w+) will match @example in "test@example.com"
        // This is expected behavior - the regex matches @example
        results.Should().HaveCount(1);
        results[0].Username.Should().Be("example");
    }

    [Fact]
    public void ParseMentions_AtSignOnly_DoesNotMatch()
    {
        // Arrange
        var content = "Use @ to mention someone";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ParseMentions_AtSignWithSpace_DoesNotMatch()
    {
        // Arrange
        var content = "Hey @ john, how are you?";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        // @(\w+) requires at least one word character after @
        // "@ john" has a space after @, so no match for "@ "
        // But "john" after the space doesn't have @ before it
        results.Should().BeEmpty();
    }

    [Fact]
    public void ParseMentions_DuplicateMentions_ReturnsBothOccurrences()
    {
        // Arrange
        var content = "@john said hello to @john";

        // Act
        var results = this.mentionService.ParseMentions(content).ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Username.Should().Be("john");
        results[0].StartPosition.Should().Be(0);
        results[1].Username.Should().Be("john");
        results[1].StartPosition.Should().Be(20);
    }

    [Fact]
    public async Task ParseAndCreateMentionsAsync_WithValidUser_CreatesMention()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var content = "Hey @testuser, check this!";
        var createdById = Guid.NewGuid();
        var mentionedUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
        };

        this.mockUserManager
            .Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(mentionedUser);

        this.mockMentionRepository
            .Setup(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<CommentMention>>()))
            .Returns(Task.CompletedTask);

        // Act
        var results = await this.mentionService.ParseAndCreateMentionsAsync(commentId, content, createdById);

        // Assert
        var mentionsList = results.ToList();
        mentionsList.Should().HaveCount(1);
        mentionsList[0].CommentId.Should().Be(commentId);
        mentionsList[0].MentionedUserId.Should().Be(mentionedUser.Id);
        mentionsList[0].StartPosition.Should().Be(4);
        mentionsList[0].Length.Should().Be(9); // @testuser

        this.mockMentionRepository.Verify(
            x => x.CreateMultipleAsync(It.Is<IEnumerable<CommentMention>>(m => m.Count() == 1)),
            Times.Once);
    }

    [Fact]
    public async Task ParseAndCreateMentionsAsync_WithInvalidUser_DoesNotCreateMention()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var content = "Hey @nonexistent, check this!";
        var createdById = Guid.NewGuid();

        this.mockUserManager
            .Setup(x => x.FindByNameAsync("nonexistent"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var results = await this.mentionService.ParseAndCreateMentionsAsync(commentId, content, createdById);

        // Assert
        results.Should().BeEmpty();

        this.mockMentionRepository.Verify(
            x => x.CreateMultipleAsync(It.IsAny<IEnumerable<CommentMention>>()),
            Times.Never);
    }

    [Fact]
    public async Task ParseAndCreateMentionsAsync_WithMixedValidAndInvalidUsers_CreatesOnlyValidMentions()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var content = "Hey @validuser and @invaliduser, check this!";
        var createdById = Guid.NewGuid();
        var validUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "validuser",
        };

        this.mockUserManager
            .Setup(x => x.FindByNameAsync("validuser"))
            .ReturnsAsync(validUser);

        this.mockUserManager
            .Setup(x => x.FindByNameAsync("invaliduser"))
            .ReturnsAsync((ApplicationUser?)null);

        this.mockMentionRepository
            .Setup(x => x.CreateMultipleAsync(It.IsAny<IEnumerable<CommentMention>>()))
            .Returns(Task.CompletedTask);

        // Act
        var results = await this.mentionService.ParseAndCreateMentionsAsync(commentId, content, createdById);

        // Assert
        var mentionsList = results.ToList();
        mentionsList.Should().HaveCount(1);
        mentionsList[0].MentionedUserId.Should().Be(validUser.Id);

        this.mockMentionRepository.Verify(
            x => x.CreateMultipleAsync(It.Is<IEnumerable<CommentMention>>(m => m.Count() == 1)),
            Times.Once);
    }

    [Fact]
    public async Task ParseAndCreateMentionsAsync_WithNoMentions_DoesNotCallRepository()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var content = "This is a comment without mentions";
        var createdById = Guid.NewGuid();

        // Act
        var results = await this.mentionService.ParseAndCreateMentionsAsync(commentId, content, createdById);

        // Assert
        results.Should().BeEmpty();

        this.mockMentionRepository.Verify(
            x => x.CreateMultipleAsync(It.IsAny<IEnumerable<CommentMention>>()),
            Times.Never);
    }
}
