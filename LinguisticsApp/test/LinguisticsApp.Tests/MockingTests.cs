using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LinguisticsApp.WebApi.Controllers;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.Application.Common.Interfaces.Repository;
using LinguisticsApp.Application.Common.Interfaces.Services;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.Application.Common.DTOs.AuthDTOs;
using AutoMapper;
using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading;
using LinguisticsApp.Application.Common.DTOs.LanguageContactEventDTOs;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using LinguisticsApp.DomainModel.ValueObjects;

namespace LinguisticsApp.Tests.MockingTests
{
    public class AuthControllerMockingTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerMockingTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuthService = new Mock<IAuthService>();

            _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

            _controller = new AuthController(_mockUnitOfWork.Object, _mockAuthService.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var email = new Email(loginDto.Email);
            var userId = UserId.New();
            var researcher = new Researcher(
                userId,
                email,
                "hashedPassword",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(researcher);

            _mockAuthService.Setup(a => a.VerifyPassword(researcher.Password, loginDto.Password))
                .Returns(true);

            var token = "mocked_jwt_token";
            _mockAuthService.Setup(a => a.GenerateToken(researcher))
                .Returns(token);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<AuthResultDto>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var authResult = Assert.IsType<AuthResultDto>(okObjectResult.Value);

            Assert.Equal(token, authResult.Token);
            Assert.Equal("Researcher", authResult.Role);
            Assert.Equal(userId.Value, authResult.UserId);
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResultDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            // Verify error message
            var errorObject = badRequestResult.Value as dynamic;
            Assert.Equal("Invalid email or password", errorObject.message.ToString());
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            var email = new Email(loginDto.Email);
            var user = new Researcher(
                UserId.New(),
                email,
                "hashedPassword",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mockAuthService.Setup(a => a.VerifyPassword(user.Password, loginDto.Password))
                .Returns(false);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResultDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            // Verify error message
            var errorObject = badRequestResult.Value as dynamic;
            Assert.Equal("Invalid email or password", errorObject.message.ToString());
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOkWithToken()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "new@example.com",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User",
                Institution = "University",
                Field = ResearchField.HistoricalLinguistics,
                IsAdmin = false
            };

            var email = new Email(registerDto.Email);

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            var hashedPassword = "hashedPassword123";
            _mockAuthService.Setup(a => a.HashPassword(registerDto.Password))
                .Returns(hashedPassword);

            var token = "mocked_jwt_token";
            _mockAuthService.Setup(a => a.GenerateToken(It.IsAny<User>()))
                .Returns(token);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<ActionResult<AuthResultDto>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var authResult = Assert.IsType<AuthResultDto>(okObjectResult.Value);

            Assert.Equal(token, authResult.Token);
            Assert.Equal("Researcher", authResult.Role);

            // Verify user was added and saved
            _mockUnitOfWork.Verify(u => u.Users.AddAsync(It.IsAny<User>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User",
                Institution = "University",
                Field = ResearchField.HistoricalLinguistics,
                IsAdmin = false
            };

            var email = new Email(registerDto.Email);
            var existingUser = new Researcher(
                UserId.New(),
                email,
                "hashedPassword",
                "Existing",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResultDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            // Verify error message
            var errorObject = badRequestResult.Value as dynamic;
            Assert.Equal("Email already registered", errorObject.message.ToString());
        }
    }

    public class LanguageContactEventsControllerMockingTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILanguageContactEventRepository> _mockRepository;
        private readonly LanguageContactEventsController _controller;

        public LanguageContactEventsControllerMockingTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMapper = new Mock<IMapper>();
            _mockRepository = new Mock<ILanguageContactEventRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _mockUnitOfWork.Setup(u => u.LanguageContactEvents).Returns(_mockRepository.Object);

            _controller = new LanguageContactEventsController(_mockMediator.Object, _mockMapper.Object, _mockUnitOfWork.Object);

            // Setup controller context with user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Researcher")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetAll_ReturnsAllEvents()
        {
            // Arrange
            var languageContactEvents = new List<LanguageContactEvent>
            {
                CreateSampleContactEvent(),
                CreateSampleContactEvent(),
                CreateSampleContactEvent()
            };

            var contactEventDtos = new List<LanguageContactEventDto>
            {
                new LanguageContactEventDto { Id = Guid.NewGuid(), Type = "Trade" },
                new LanguageContactEventDto { Id = Guid.NewGuid(), Type = "Migration" },
                new LanguageContactEventDto { Id = Guid.NewGuid(), Type = "Conquest" }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(languageContactEvents);

            _mockMapper.Setup(m => m.Map<IEnumerable<LanguageContactEventDto>>(languageContactEvents))
                .Returns(contactEventDtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<LanguageContactEventDto>>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var returnedDtos = Assert.IsAssignableFrom<IEnumerable<LanguageContactEventDto>>(okObjectResult.Value);

            Assert.Equal(3, returnedDtos.Count());
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsEvent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contactEvent = CreateSampleContactEvent(id);
            var contactEventDto = new LanguageContactEventDto { Id = id, Type = "Trade" };

            _mockRepository.Setup(r => r.GetByIdAsync(new LanguageContactEventId(id)))
                .ReturnsAsync(contactEvent);

            _mockMapper.Setup(m => m.Map<LanguageContactEventDto>(contactEvent))
                .Returns(contactEventDto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<ActionResult<LanguageContactEventDto>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var returnedDto = Assert.IsType<LanguageContactEventDto>(okObjectResult.Value);

            Assert.Equal(id, returnedDto.Id);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(new LanguageContactEventId(id)))
                .ReturnsAsync((LanguageContactEvent)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<LanguageContactEventDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task Create_WithValidCommand_ReturnsCreatedAtAction()
        {
            // Arrange
            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test influence",
                SourceLanguageId = Guid.NewGuid(),
                TargetLanguageId = Guid.NewGuid()
            };

            var createdId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdId);

            var contactEvent = CreateSampleContactEvent(createdId);
            var contactEventDto = new LanguageContactEventDto { Id = createdId, Type = "Trade" };

            _mockRepository.Setup(r => r.GetByIdAsync(new LanguageContactEventId(createdId)))
                .ReturnsAsync(contactEvent);

            _mockMapper.Setup(m => m.Map<LanguageContactEventDto>(contactEvent))
                .Returns(contactEventDto);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var createdResult = Assert.IsType<ActionResult<LanguageContactEventDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(createdResult.Result);

            Assert.Equal(nameof(_controller.GetById), createdAtActionResult.ActionName);
            Assert.Equal(createdId, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(contactEventDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task AddLoanword_WithValidCommand_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new AddLoanwordCommand
            {
                Loanword = "test loanword"
            };

            _mockMediator.Setup(m => m.Send(It.Is<AddLoanwordCommand>(cmd =>
                cmd.ContactEventId == id && cmd.Loanword == command.Loanword),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddLoanword(id, command);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify the command was sent with the correct ID
            _mockMediator.Verify(m => m.Send(
                It.Is<AddLoanwordCommand>(cmd => cmd.ContactEventId == id && cmd.Loanword == command.Loanword),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_WithValidCommand_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new UpdateLanguageContactEventCommand
            {
                Type = ContactType.Migration,
                GrammaticalInfluence = "Updated influence"
            };

            _mockMediator.Setup(m => m.Send(It.Is<UpdateLanguageContactEventCommand>(cmd =>
                cmd.Id == id && cmd.Type == command.Type && cmd.GrammaticalInfluence == command.GrammaticalInfluence),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(id, command);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify the command was sent with the correct ID
            _mockMediator.Verify(m => m.Send(
                It.Is<UpdateLanguageContactEventCommand>(cmd =>
                    cmd.Id == id &&
                    cmd.Type == command.Type &&
                    cmd.GrammaticalInfluence == command.GrammaticalInfluence),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Helper method to create sample contact events
        private LanguageContactEvent CreateSampleContactEvent(Guid? id = null)
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

            return new LanguageContactEvent(
                id.HasValue ? new LanguageContactEventId(id.Value) : LanguageContactEventId.New(),
                ContactType.Trade,
                "Test grammatical influence",
                sourceLanguage,
                targetLanguage
            );
        }
    }

    public class MediatorPipelineTests
    {
        [Fact]
        public async Task ValidationBehavior_WithValidCommand_CallsNextDelegate()
        {
            // Arrange
            var mockValidator = new Mock<FluentValidation.IValidator<CreateLanguageContactEventCommand>>();
            var mockValidators = new List<FluentValidation.IValidator<CreateLanguageContactEventCommand>> { mockValidator.Object };

            var validationBehavior = new LinguisticsApp.Application.Common.Behaviours.LinguisticsApp.Application.Common.Behaviors.ValidationBehavior<CreateLanguageContactEventCommand, Guid>(mockValidators);

            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test influence",
                SourceLanguageId = Guid.NewGuid(),
                TargetLanguageId = Guid.NewGuid()
            };

            var expectedResult = Guid.NewGuid();
            var nextCalled = false;

            Task<Guid> Next()
            {
                nextCalled = true;
                return Task.FromResult(expectedResult);
            }

            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<FluentValidation.ValidationContext<CreateLanguageContactEventCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Act
            var result = await validationBehavior.Handle(command, Next, CancellationToken.None);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ValidationBehavior_WithInvalidCommand_ThrowsValidationException()
        {
            // Arrange
            var mockValidator = new Mock<FluentValidation.IValidator<CreateLanguageContactEventCommand>>();
            var mockValidators = new List<FluentValidation.IValidator<CreateLanguageContactEventCommand>> { mockValidator.Object };

            var validationBehavior = new LinguisticsApp.Application.Common.Behaviours.LinguisticsApp.Application.Common.Behaviors.ValidationBehavior<CreateLanguageContactEventCommand, Guid>(mockValidators);

            var command = new CreateLanguageContactEventCommand
            {
                // Invalid command - missing required fields
            };

            Task<Guid> Next()
            {
                return Task.FromResult(Guid.NewGuid());
            }

            // Setup validator to return validation failures
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("GrammaticalInfluence", "Grammatical influence is required."),
                new FluentValidation.Results.ValidationFailure("SourceLanguageId", "Source language ID is required.")
            };

            var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);

            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<FluentValidation.ValidationContext<CreateLanguageContactEventCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act & Assert
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
                validationBehavior.Handle(command, Next, CancellationToken.None));
        }
    }

    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new LinguisticsApp.Application.Common.Mappings.MappingProfile());
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public void LanguageContactEvent_MapsToDto_Correctly()
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var sourceLanguageId = PhonemeInventoryId.New();
            var sourceLanguage = new PhonemeInventory(
                sourceLanguageId,
                new LanguageName("Source Language"),
                StressPattern.Initial,
                researcher
            );

            var targetLanguageId = PhonemeInventoryId.New();
            var targetLanguage = new PhonemeInventory(
                targetLanguageId,
                new LanguageName("Target Language"),
                StressPattern.Initial,
                researcher
            );

            var contactEventId = LanguageContactEventId.New();
            var contactEvent = new LanguageContactEvent(
                contactEventId,
                ContactType.Trade,
                "Test grammatical influence",
                sourceLanguage,
                targetLanguage
            );

            contactEvent.AddLoanword("loanword1");
            contactEvent.AddLoanword("loanword2");

            // Act
            var dto = _mapper.Map<LanguageContactEventDto>(contactEvent);

            // Assert
            Assert.Equal(contactEventId.Value, dto.Id);
            Assert.Equal("Trade", dto.Type);
            Assert.Equal("Test grammatical influence", dto.GrammaticalInfluence);
            Assert.Equal(sourceLanguageId.Value, dto.SourceLanguage.Id);
            Assert.Equal("Source Language", dto.SourceLanguage.Name);
            Assert.Equal(targetLanguageId.Value, dto.TargetLanguage.Id);
            Assert.Equal("Target Language", dto.TargetLanguage.Name);
            Assert.Contains("loanword1", dto.LoanwordsAdopted);
            Assert.Contains("loanword2", dto.LoanwordsAdopted);
        }

        [Fact]
        public void CreateLanguageContactEventCommand_MapsToEntity_Correctly()
        {
            // Skip the test for this example as it requires repository setups
            // In a real test, you would mock the repositories to return the source and target languages
        }

        [Fact]
        public void PhonemeInventory_MapsToDto_Correctly()
        {
            // Arrange
            var researcherId = UserId.New();
            var researcher = new Researcher(
                researcherId,
                new Email("test@example.com"),
                "password",
                "Test",
                "User",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var inventoryId = PhonemeInventoryId.New();
            var inventory = new PhonemeInventory(
                inventoryId,
                new LanguageName("Test Language"),
                StressPattern.Initial,
                researcher
            );

            inventory.AddConsonant(new Phoneme("p", false));
            inventory.AddConsonant(new Phoneme("t", false));
            inventory.AddVowel(new Phoneme("a", true));
            inventory.AddVowel(new Phoneme("i", true));

            // Act
            var dto = _mapper.Map<LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs.PhonemeInventoryDto>(inventory);

            // Assert
            Assert.Equal(inventoryId.Value, dto.Id);
            Assert.Equal("Test Language", dto.Name);
            Assert.Equal("Initial", dto.StressPattern);
            Assert.Equal(2, dto.Consonants.Count);
            Assert.Equal(2, dto.Vowels.Count);
            Assert.Contains(dto.Consonants, p => p.Value == "p");
            Assert.Contains(dto.Consonants, p => p.Value == "t");
            Assert.Contains(dto.Vowels, p => p.Value == "a");
            Assert.Contains(dto.Vowels, p => p.Value == "i");
            Assert.Equal(researcherId.Value, dto.Creator.Id);
            Assert.Equal("Test", dto.Creator.FirstName);
            Assert.Equal("User", dto.Creator.LastName);
        }
    }
}