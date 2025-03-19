using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MediatR;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using LinguisticsApp.Application.Common.Handlers.LanguageContactEventHandlers;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.Application.Common.Interfaces.Repository;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.Application.Common.Interfaces.Services;
using AutoMapper;
using LinguisticsApp.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using LinguisticsApp.Infrastructure.Service.Auth;
using System.Collections.Generic;
using LinguisticsApp.Application.Common.Exceptions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace LinguisticsApp.Tests.Handlers
{
    public class AddLoanwordHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILanguageContactEventRepository> _mockRepository;
        private readonly AddLoanwordHandler _handler;

        public AddLoanwordHandlerTests()
        {
            _mockRepository = new Mock<ILanguageContactEventRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.LanguageContactEvents).Returns(_mockRepository.Object);
            _handler = new AddLoanwordHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_AddsLoanwordToEvent()
        {
            // Arrange
            var command = new AddLoanwordCommand
            {
                ContactEventId = Guid.NewGuid(),
                Loanword = "test loanword"
            };

            var contactEventId = new LanguageContactEventId(command.ContactEventId);
            var researcher = new Researcher(
                UserId.New(),
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var sourceLanguage = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Source Language"),
                StressPattern.Initial,
                researcher
            );

            var targetLanguage = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Target Language"),
                StressPattern.Initial,
                researcher
            );

            var contactEvent = new LanguageContactEvent(
                contactEventId,
                ContactType.Trade,
                "Test influence",
                sourceLanguage,
                targetLanguage
            );

            _mockRepository.Setup(r => r.GetByIdAsync(contactEventId))
                .ReturnsAsync(contactEvent);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Contains("test loanword", contactEvent.LoanwordsAdopted);
        }

        [Fact]
        public async Task Handle_WithInvalidEventId_ThrowsNotFoundException()
        {
            // Arrange
            var command = new AddLoanwordCommand
            {
                ContactEventId = Guid.NewGuid(),
                Loanword = "test loanword"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<LanguageContactEventId>()))
                .ReturnsAsync((LanguageContactEvent)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }

    public class CreateLanguageContactEventHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPhonemeInventoryRepository> _mockPhonemeInventoryRepository;
        private readonly Mock<ILanguageContactEventRepository> _mockLanguageContactEventRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CreateLanguageContactEventHandler _handler;

        public CreateLanguageContactEventHandlerTests()
        {
            _mockPhonemeInventoryRepository = new Mock<IPhonemeInventoryRepository>();
            _mockLanguageContactEventRepository = new Mock<ILanguageContactEventRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _mockUnitOfWork.Setup(u => u.PhonemeInventories).Returns(_mockPhonemeInventoryRepository.Object);
            _mockUnitOfWork.Setup(u => u.LanguageContactEvents).Returns(_mockLanguageContactEventRepository.Object);

            _handler = new CreateLanguageContactEventHandler(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_CreatesAndReturnsId()
        {
            // Arrange
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test influence",
                SourceLanguageId = sourceId,
                TargetLanguageId = targetId,
                LoanwordsAdopted = new List<string> { "loanword1", "loanword2" }
            };

            var researcher = new Researcher(
                UserId.New(),
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var sourceLanguage = new PhonemeInventory(
                new PhonemeInventoryId(sourceId),
                new LanguageName("Source Language"),
                StressPattern.Initial,
                researcher
            );

            var targetLanguage = new PhonemeInventory(
                new PhonemeInventoryId(targetId),
                new LanguageName("Target Language"),
                StressPattern.Initial,
                researcher
            );

            _mockPhonemeInventoryRepository.Setup(r => r.GetByIdAsync(new PhonemeInventoryId(sourceId)))
                .ReturnsAsync(sourceLanguage);
            _mockPhonemeInventoryRepository.Setup(r => r.GetByIdAsync(new PhonemeInventoryId(targetId)))
                .ReturnsAsync(targetLanguage);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockLanguageContactEventRepository.Verify(r => r.AddAsync(It.IsAny<LanguageContactEvent>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidSourceLanguageId_ThrowsNotFoundException()
        {
            // Arrange
            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test influence",
                SourceLanguageId = Guid.NewGuid(),
                TargetLanguageId = Guid.NewGuid()
            };

            _mockPhonemeInventoryRepository.Setup(r => r.GetByIdAsync(It.IsAny<PhonemeInventoryId>()))
                .ReturnsAsync((PhonemeInventory)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }

    public class UpdateLanguageContactEventHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILanguageContactEventRepository> _mockRepository;
        private readonly UpdateLanguageContactEventHandler _handler;

        public UpdateLanguageContactEventHandlerTests()
        {
            _mockRepository = new Mock<ILanguageContactEventRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.LanguageContactEvents).Returns(_mockRepository.Object);
            _handler = new UpdateLanguageContactEventHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_UpdatesEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var command = new UpdateLanguageContactEventCommand
            {
                Id = eventId,
                Type = ContactType.Migration,
                GrammaticalInfluence = "Updated influence"
            };

            var researcher = new Researcher(
                UserId.New(),
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var sourceLanguage = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Source Language"),
                StressPattern.Initial,
                researcher
            );

            var targetLanguage = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Target Language"),
                StressPattern.Initial,
                researcher
            );

            var contactEvent = new LanguageContactEvent(
                new LanguageContactEventId(eventId),
                ContactType.Trade,
                "Original influence",
                sourceLanguage,
                targetLanguage
            );

            _mockRepository.Setup(r => r.GetByIdAsync(new LanguageContactEventId(eventId)))
                .ReturnsAsync(contactEvent);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(ContactType.Migration, contactEvent.Type);
            Assert.Equal("Updated influence", contactEvent.GrammaticalInfluence);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidEventId_ThrowsNotFoundException()
        {
            // Arrange
            var command = new UpdateLanguageContactEventCommand
            {
                Id = Guid.NewGuid(),
                Type = ContactType.Migration
            };

            _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<LanguageContactEventId>()))
                .ReturnsAsync((LanguageContactEvent)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithPartialUpdate_OnlyUpdatesProvidedFields()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var command = new UpdateLanguageContactEventCommand
            {
                Id = eventId,
                Type = ContactType.Migration,
                // GrammaticalInfluence is not provided
            };

            var researcher = new Researcher(
                UserId.New(),
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var sourceLanguage = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Source Language"),
                StressPattern.Initial,
                researcher
            );

            var targetLanguage = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Target Language"),
                StressPattern.Initial,
                researcher
            );

            var contactEvent = new LanguageContactEvent(
                new LanguageContactEventId(eventId),
                ContactType.Trade,
                "Original influence",
                sourceLanguage,
                targetLanguage
            );

            _mockRepository.Setup(r => r.GetByIdAsync(new LanguageContactEventId(eventId)))
                .ReturnsAsync(contactEvent);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(ContactType.Migration, contactEvent.Type);
            Assert.Equal("Original influence", contactEvent.GrammaticalInfluence);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }

    public class JwtAuthServiceTests
    {
        private readonly JwtAuthService _authService;
        private readonly JwtAuthOptions _options;

        public JwtAuthServiceTests()
        {
            _options = new JwtAuthOptions
            {
                SecretKey = "This_is_a_very_secret_key_for_testing_that_is_long_enough_for_jwt_signing",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                TokenExpirationMinutes = 30
            };

            var mockOptions = new Mock<IOptions<JwtAuthOptions>>();
            mockOptions.Setup(o => o.Value).Returns(_options);

            _authService = new JwtAuthService(mockOptions.Object);
        }

        [Fact]
        public void HashPassword_ReturnsDifferentHashForSamePassword()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hash1 = _authService.HashPassword(password);
            var hash2 = _authService.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // Hashes should be different due to random salt
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "TestPassword123!";
            var hashedPassword = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(hashedPassword, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var password = "TestPassword123!";
            var wrongPassword = "WrongPassword123!";
            var hashedPassword = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(hashedPassword, wrongPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GenerateToken_ProducesValidToken()
        {
            // Arrange
            var userId = UserId.New();
            var user = new Researcher(
                userId,
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            // Act
            var token = _authService.GenerateToken(user);

            // Assert
            Assert.NotEmpty(token);

            // Verify token can be validated
            var principal = _authService.GetPrincipalFromToken(token);
            Assert.NotNull(principal);

            var userIdClaim = principal.FindFirst("sub");
            Assert.NotNull(userIdClaim);
            Assert.Equal(userId.Value.ToString(), userIdClaim.Value);
        }

        [Fact]
        public async Task ValidateToken_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var userId = UserId.New();
            var user = new Researcher(
                userId,
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var token = _authService.GenerateToken(user);

            // Act
            var result = await _authService.ValidateTokenAsync(token);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(LinguisticsApp.Application.Common.UserRole.Researcher, result.Role);
        }

        [Fact]
        public async Task ValidateToken_WithInvalidToken_ReturnsFailure()
        {
            // Arrange
            var invalidToken = "invalid.token.string";

            // Act
            var result = await _authService.ValidateTokenAsync(invalidToken);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotNull(result.ErrorMessage);
        }
    }

    public class ValidationTests
    {
        [Fact]
        public void CreateLanguageContactEventCommandValidator_ValidCommand_PassesValidation()
        {
            // Arrange
            var validator = new LinguisticsApp.Application.Common.Validators.CreateLanguageContactEventCommandValidator();
            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test grammatical influence",
                SourceLanguageId = Guid.NewGuid(),
                TargetLanguageId = Guid.NewGuid(),
                LoanwordsAdopted = new List<string> { "word1", "word2" }
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void CreateLanguageContactEventCommandValidator_EmptyGrammaticalInfluence_FailsValidation()
        {
            // Arrange
            var validator = new LinguisticsApp.Application.Common.Validators.CreateLanguageContactEventCommandValidator();
            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "", // Empty influence
                SourceLanguageId = Guid.NewGuid(),
                TargetLanguageId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "GrammaticalInfluence");
        }

        [Fact]
        public void CreateLanguageContactEventCommandValidator_SameSourceAndTargetLanguage_FailsValidation()
        {
            // Arrange
            var validator = new LinguisticsApp.Application.Common.Validators.CreateLanguageContactEventCommandValidator();
            var languageId = Guid.NewGuid();
            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test influence",
                SourceLanguageId = languageId,
                TargetLanguageId = languageId // Same as source
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "TargetLanguageId");
        }

        [Fact]
        public void CreateLanguageContactEventCommandValidator_EmptyLoanword_FailsValidation()
        {
            // Arrange
            var validator = new LinguisticsApp.Application.Common.Validators.CreateLanguageContactEventCommandValidator();
            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test influence",
                SourceLanguageId = Guid.NewGuid(),
                TargetLanguageId = Guid.NewGuid(),
                LoanwordsAdopted = new List<string> { "word1", "" } // Empty loanword
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName.Contains("LoanwordsAdopted"));
        }
    }

    public class RepositoryTests
    {
        // These tests check the repository implementations using in-memory database

        [Fact]
        public async Task LanguageContactEventRepository_GetByLanguageAsync_ReturnsCorrectEvents()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;

            var languageId = PhonemeInventoryId.New();

            // Setup data
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var researcher = new Researcher(
                    UserId.New(),
                    new Email("test@example.com"),
                    "password",
                    "Test",
                    "User",
                    "University",
                    ResearchField.HistoricalLinguistics
                );

                context.Researchers.Add(researcher);
                await context.SaveChangesAsync();

                var sourceLanguage = new PhonemeInventory(
                    languageId,
                    new LanguageName("Source Language"),
                    StressPattern.Initial,
                    researcher
                );

                var targetLanguage = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Target Language"),
                    StressPattern.Initial,
                    researcher
                );

                context.PhonemeInventories.Add(sourceLanguage);
                context.PhonemeInventories.Add(targetLanguage);
                await context.SaveChangesAsync();

                // Create two events with the target language
                var event1 = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    ContactType.Trade,
                    "Influence 1",
                    sourceLanguage,
                    targetLanguage
                );

                var event2 = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    ContactType.Migration,
                    "Influence 2",
                    sourceLanguage,
                    targetLanguage
                );

                // Create one event with a different language
                var event3 = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    ContactType.Conquest,
                    "Influence 3",
                    targetLanguage,
                    sourceLanguage
                );

                context.ContactEvents.Add(event1);
                context.ContactEvents.Add(event2);
                context.ContactEvents.Add(event3);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var repository = new LinguisticsApp.Infrastructure.Repository.LanguageContactEventRepository(context);
                var events = await repository.GetByLanguageAsync(languageId);

                // Assert
                Assert.Equal(3, events.Count()); // All events involve the source language
            }
        }

        [Fact]
        public async Task LanguageContactEventRepository_GetByContactTypeAsync_ReturnsCorrectEvents()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;

            // Setup data
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var researcher = new Researcher(
                    UserId.New(),
                    new Email("test@example.com"),
                    "password",
                    "Test",
                    "User",
                    "University",
                    ResearchField.HistoricalLinguistics
                );

                context.Researchers.Add(researcher);
                await context.SaveChangesAsync();

                var sourceLanguage = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Source Language"),
                    StressPattern.Initial,
                    researcher
                );

                var targetLanguage = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Target Language"),
                    StressPattern.Initial,
                    researcher
                );

                context.PhonemeInventories.Add(sourceLanguage);
                context.PhonemeInventories.Add(targetLanguage);
                await context.SaveChangesAsync();

                // Create events with different contact types
                var event1 = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    ContactType.Trade,
                    "Influence 1",
                    sourceLanguage,
                    targetLanguage
                );

                var event2 = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    ContactType.Trade,
                    "Influence 2",
                    sourceLanguage,
                    targetLanguage
                );

                var event3 = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    ContactType.Conquest,
                    "Influence 3",
                    sourceLanguage,
                    targetLanguage
                );

                context.ContactEvents.Add(event1);
                context.ContactEvents.Add(event2);
                context.ContactEvents.Add(event3);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var repository = new LinguisticsApp.Infrastructure.Repository.LanguageContactEventRepository(context);
                var events = await repository.GetByContactTypeAsync(ContactType.Trade);

                // Assert
                Assert.Equal(2, events.Count()); // 2 trade events
                Assert.All(events, e => Assert.Equal(ContactType.Trade, e.Type));
            }
        }

        [Fact]
        public async Task PhonemeInventoryRepository_GetByCreatorAsync_ReturnsCorrectInventories()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;

            var researcher1Id = UserId.New();
            var researcher2Id = UserId.New();

            // Setup data
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var researcher1 = new Researcher(
                    researcher1Id,
                    new Email("researcher1@example.com"),
                    "password",
                    "Researcher",
                    "One",
                    "University 1",
                    ResearchField.HistoricalLinguistics
                );

                var researcher2 = new Researcher(
                    researcher2Id,
                    new Email("researcher2@example.com"),
                    "password",
                    "Researcher",
                    "Two",
                    "University 2",
                    ResearchField.Phonology
                );

                context.Researchers.Add(researcher1);
                context.Researchers.Add(researcher2);
                await context.SaveChangesAsync();

                // Create inventories for each researcher
                var inventory1 = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Language 1"),
                    StressPattern.Initial,
                    researcher1
                );

                var inventory2 = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Language 2"),
                    StressPattern.Penultimate,
                    researcher1
                );

                var inventory3 = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Language 3"),
                    StressPattern.Ultimate,
                    researcher2
                );

                context.PhonemeInventories.Add(inventory1);
                context.PhonemeInventories.Add(inventory2);
                context.PhonemeInventories.Add(inventory3);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var repository = new LinguisticsApp.Infrastructure.Repository.PhonemeInventoryRepository(context);
                var inventories = await repository.GetByCreatorAsync(researcher1Id);

                // Assert
                Assert.Equal(2, inventories.Count()); // 2 inventories for researcher1
                Assert.All(inventories, i => Assert.Equal(researcher1Id, i.Creator.Id));
            }
        }

        [Fact]
        public async Task PhonemeInventoryRepository_GetByLanguageNameAsync_ReturnsCorrectInventories()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;

            // Setup data
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var researcher = new Researcher(
                    UserId.New(),
                    new Email("researcher@example.com"),
                    "password",
                    "Researcher",
                    "Name",
                    "University",
                    ResearchField.HistoricalLinguistics
                );

                context.Researchers.Add(researcher);
                await context.SaveChangesAsync();

                // Create inventories with different names
                var inventory1 = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Proto-Germanic"),
                    StressPattern.Initial,
                    researcher
                );

                var inventory2 = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Old Germanic"),
                    StressPattern.Penultimate,
                    researcher
                );

                var inventory3 = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Proto-Slavic"),
                    StressPattern.Ultimate,
                    researcher
                );

                context.PhonemeInventories.Add(inventory1);
                context.PhonemeInventories.Add(inventory2);
                context.PhonemeInventories.Add(inventory3);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new LinguisticsApp.Infrastructure.Persistence.LinguisticsDbContext(options))
            {
                var repository = new LinguisticsApp.Infrastructure.Repository.PhonemeInventoryRepository(context);
                var inventories = await repository.GetByLanguageNameAsync(new LanguageName("Germanic"));

                // Assert
                Assert.Equal(2, inventories.Count()); // Both Proto-Germanic and Old Germanic should be returned
                Assert.Contains(inventories, i => i.Name.Value == "Proto-Germanic");
                Assert.Contains(inventories, i => i.Name.Value == "Old Germanic");
            }
        }
    }
}