using Xunit;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using LinguisticsApp.Application.Common.DTOs.AuthDTOs;
using LinguisticsApp.Application.Common.DTOs.LanguageContactEventDTOs;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using LinguisticsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.Enumerations;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace LinguisticsApp.Tests.ControllerTests
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace DB context with in-memory database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<LinguisticsDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<LinguisticsDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForAuthTesting");
                    });
                });
            }).CreateClient();

            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Institution = "Test University",
                Field = ResearchField.HistoricalLinguistics,
                IsAdmin = false
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadFromJsonAsync<AuthResultDto>();
            Assert.NotNull(responseContent);
            Assert.Equal("Researcher", responseContent.Role);
            Assert.NotEmpty(responseContent.Token);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace DB context with in-memory database 
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<LinguisticsDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<LinguisticsDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForLoginTesting");
                    });

                    // Create a scope to obtain a reference to the database context
                    using (var scope = services.BuildServiceProvider().CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<LinguisticsDbContext>();
                        var authService = scopedServices.GetRequiredService<LinguisticsApp.Application.Common.Interfaces.Services.IAuthService>();

                        // Ensure database is created and add test user
                        db.Database.EnsureCreated();

                        // Add a user
                        var researcher = new Researcher(
                            UserId.New(),
                            new Email("login@example.com"),
                            authService.HashPassword("Password123!"),
                            "Login",
                            "User",
                            "Test Institution",
                            ResearchField.HistoricalLinguistics
                        );

                        db.Researchers.Add(researcher);
                        db.SaveChanges();
                    }
                });
            }).CreateClient();

            var loginDto = new LoginDto
            {
                Email = "login@example.com",
                Password = "Password123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadFromJsonAsync<AuthResultDto>();
            Assert.NotNull(responseContent);
            Assert.Equal("Researcher", responseContent.Role);
            Assert.NotEmpty(responseContent.Token);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    public class LanguageContactEventsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public LanguageContactEventsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();

            // Set authorization header with test token
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer TestToken");
        }

        [Fact]
        public async Task GetAll_ReturnsSuccessAndCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync("/api/LanguageContactEvents");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsLanguageContactEvent()
        {
            // Arrange
            var id = await SetupTestLanguageContactEvent();

            // Act
            var response = await _client.GetAsync($"/api/LanguageContactEvents/{id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var contactEvent = await response.Content.ReadFromJsonAsync<LanguageContactEventDto>();
            Assert.NotNull(contactEvent);
            Assert.Equal(id, contactEvent.Id);
            Assert.Equal("Trade", contactEvent.Type);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/LanguageContactEvents/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithValidData_ReturnsCreated()
        {
            // Arrange
            // Create source and target languages first
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<LinguisticsDbContext>();

                var researcher = dbContext.Researchers.First();

                // Add two phoneme inventories
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

                dbContext.PhonemeInventories.Add(sourceLanguage);
                dbContext.PhonemeInventories.Add(targetLanguage);
                await dbContext.SaveChangesAsync();
            }

            var command = new CreateLanguageContactEventCommand
            {
                Type = ContactType.Trade,
                GrammaticalInfluence = "Test grammatical influence",
                SourceLanguageId = sourceId,
                TargetLanguageId = targetId,
                LoanwordsAdopted = new List<string> { "test1", "test2" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/LanguageContactEvents", command);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Verify we can get the location of the new resource
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task AddLoanword_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var id = await SetupTestLanguageContactEvent();

            var command = new AddLoanwordCommand
            {
                Loanword = "new loanword"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/LanguageContactEvents/{id}/loanwords", command);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the loanword was added
            var getResponse = await _client.GetAsync($"/api/LanguageContactEvents/{id}");
            var contactEvent = await getResponse.Content.ReadFromJsonAsync<LanguageContactEventDto>();
            Assert.Contains("new loanword", contactEvent.LoanwordsAdopted);
        }

        [Fact]
        public async Task Update_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var id = await SetupTestLanguageContactEvent();

            var updateCommand = new UpdateLanguageContactEventCommand
            {
                Type = ContactType.Migration,
                GrammaticalInfluence = "Updated grammatical influence"
            };

            // Act
            var response = await _client.PatchAsJsonAsync($"/api/LanguageContactEvents/{id}", updateCommand);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update was applied
            var getResponse = await _client.GetAsync($"/api/LanguageContactEvents/{id}");
            var contactEvent = await getResponse.Content.ReadFromJsonAsync<LanguageContactEventDto>();
            Assert.Equal("Migration", contactEvent.Type);
            Assert.Equal("Updated grammatical influence", contactEvent.GrammaticalInfluence);
        }

        private async Task<Guid> SetupTestLanguageContactEvent()
        {
            // Create a test language contact event
            Guid contactEventId;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<LinguisticsDbContext>();

                var researcher = dbContext.Researchers.FirstOrDefault();
                if (researcher == null)
                {
                    // Create a researcher if none exists
                    researcher = new Researcher(
                        UserId.New(),
                        new Email("test@example.com"),
                        "password",
                        "Test",
                        "User",
                        "Test Institution",
                        ResearchField.HistoricalLinguistics
                    );
                    dbContext.Researchers.Add(researcher);
                    await dbContext.SaveChangesAsync();
                }

                // Create source and target languages
                var sourceLanguage = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Test Source Language"),
                    StressPattern.Initial,
                    researcher
                );

                var targetLanguage = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName("Test Target Language"),
                    StressPattern.Initial,
                    researcher
                );

                dbContext.PhonemeInventories.Add(sourceLanguage);
                dbContext.PhonemeInventories.Add(targetLanguage);
                await dbContext.SaveChangesAsync();

                // Create a language contact event
                var contactEvent = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    ContactType.Trade,
                    "Test grammatical influence",
                    sourceLanguage,
                    targetLanguage
                );

                contactEvent.AddLoanword("test loanword");

                dbContext.ContactEvents.Add(contactEvent);
                await dbContext.SaveChangesAsync();

                contactEventId = contactEvent.Id.Value;
            }

            return contactEventId;
        }
    }

    // Custom WebApplicationFactory for testing with authentication
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<LinguisticsDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<LinguisticsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Add test authentication handler
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<LinguisticsDbContext>();

                    // Ensure database is created and seed with test data
                    db.Database.EnsureCreated();
                    SeedTestData(db);
                }
            });
        }

        private void SeedTestData(LinguisticsDbContext db)
        {
            // Add a test researcher
            var researcher = new Researcher(
                UserId.New(),
                new Email("testresearcher@example.com"),
                "password",
                "Test",
                "Researcher",
                "University of Testing",
                ResearchField.HistoricalLinguistics
            );

            db.Researchers.Add(researcher);
            db.SaveChanges();
        }
    }

    // Test authentication handler for controller tests
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Researcher")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    // Extension methods for HttpClient
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            var content = JsonContent.Create(value);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return client.SendAsync(request);
        }
    }
}