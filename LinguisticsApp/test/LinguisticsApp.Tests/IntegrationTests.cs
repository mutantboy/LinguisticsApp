using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using LinguisticsApp.Application.Common.DTOs.AuthDTOs;
using LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.ValueObjects;
using LinguisticsApp.Infrastructure.Persistence;
using LinguisticsApp.Application.Common.Commands.PhonemeInventoryCommands;
using LinguisticsApp.Application.Common.Commands.CognateSetCommands;
using LinguisticsApp.Application.Common.DTOs.CognateSetDTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace LinguisticsApp.Tests.IntegrationTests
{
    public class PhonemeInventoryIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string _authToken;

        public PhonemeInventoryIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task AuthenticateAsync()
        {
            // Authenticate to get a token
            var loginDto = new LoginDto
            {
                Email = "testresearcher@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            response.EnsureSuccessStatusCode();

            var authResult = await response.Content.ReadFromJsonAsync<AuthResultDto>();
            _authToken = authResult.Token;

            // Set authorization header for subsequent requests
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        }

        [Fact]
        public async Task CreatePhonemeInventory_GetAndUpdate_EndToEndTest()
        {
            // Arrange
            await AuthenticateAsync();

            var command = new CreatePhonemeInventoryCommand
            {
                Name = "Test Language",
                StressPattern = StressPattern.Initial,
                Consonants = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "p" },
                    new PhonemeCommand { Value = "t" },
                    new PhonemeCommand { Value = "k" }
                },
                Vowels = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "a" },
                    new PhonemeCommand { Value = "i" },
                    new PhonemeCommand { Value = "u" }
                }
            };

            // Act - Create the phoneme inventory
            var createResponse = await _client.PostAsJsonAsync("/api/PhonemeInventories", command);

            // Assert - Creation was successful
            createResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Get the location of the created resource
            var location = createResponse.Headers.Location.ToString();
            var id = location.Substring(location.LastIndexOf('/') + 1);

            // Act - Get the created phoneme inventory
            var getResponse = await _client.GetAsync($"/api/PhonemeInventories/{id}");

            // Assert - Get was successful
            getResponse.EnsureSuccessStatusCode();
            var inventory = await getResponse.Content.ReadFromJsonAsync<PhonemeInventoryDto>();

            Assert.Equal("Test Language", inventory.Name);
            Assert.Equal("Initial", inventory.StressPattern);
            Assert.Equal(3, inventory.Consonants.Count);
            Assert.Equal(3, inventory.Vowels.Count);

            // Act - Update the phoneme inventory
            var updateCommand = new UpdatePhonemeInventoryCommand
            {
                Name = "Updated Language Name",
                StressPattern = StressPattern.Penultimate
            };

            var updateResponse = await _client.PatchAsJsonAsync($"/api/PhonemeInventories/{id}", updateCommand);

            // Assert - Update was successful
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // Act - Get the updated phoneme inventory
            var getUpdatedResponse = await _client.GetAsync($"/api/PhonemeInventories/{id}");

            // Assert - Get was successful and updates were applied
            getUpdatedResponse.EnsureSuccessStatusCode();
            var updatedInventory = await getUpdatedResponse.Content.ReadFromJsonAsync<PhonemeInventoryDto>();

            Assert.Equal("Updated Language Name", updatedInventory.Name);
            Assert.Equal("Penultimate", updatedInventory.StressPattern);

            // Act - Add a phoneme to the inventory
            var addPhonemeCommand = new AddPhonemeCommand
            {
                Value = "m",
                IsVowel = false
            };

            var addPhonemeResponse = await _client.PostAsJsonAsync($"/api/PhonemeInventories/{id}/phonemes", addPhonemeCommand);

            // Assert - Add phoneme was successful
            Assert.Equal(HttpStatusCode.NoContent, addPhonemeResponse.StatusCode);

            // Act - Get the inventory with the new phoneme
            var getFinalResponse = await _client.GetAsync($"/api/PhonemeInventories/{id}");

            // Assert - Get was successful and phoneme was added
            getFinalResponse.EnsureSuccessStatusCode();
            var finalInventory = await getFinalResponse.Content.ReadFromJsonAsync<PhonemeInventoryDto>();

            Assert.Equal(4, finalInventory.Consonants.Count);
            Assert.Contains(finalInventory.Consonants, p => p.Value == "m");
        }
    }

    public class CognateSetIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string _authToken;
        private Guid _phonemeInventoryId;

        public CognateSetIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            SetupTestDataAsync().Wait();
        }

        private async Task SetupTestDataAsync()
        {
            // Authenticate to get a token
            var loginDto = new LoginDto
            {
                Email = "testresearcher@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            response.EnsureSuccessStatusCode();

            var authResult = await response.Content.ReadFromJsonAsync<AuthResultDto>();
            _authToken = authResult.Token;

            // Set authorization header for subsequent requests
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");

            // Create a phoneme inventory to use in tests
            var command = new CreatePhonemeInventoryCommand
            {
                Name = "Proto-Language",
                StressPattern = StressPattern.Initial,
                Consonants = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "p" },
                    new PhonemeCommand { Value = "t" },
                    new PhonemeCommand { Value = "k" }
                },
                Vowels = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "a" },
                    new PhonemeCommand { Value = "i" },
                    new PhonemeCommand { Value = "u" }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/PhonemeInventories", command);
            createResponse.EnsureSuccessStatusCode();

            var location = createResponse.Headers.Location.ToString();
            _phonemeInventoryId = Guid.Parse(location.Substring(location.LastIndexOf('/') + 1));
        }

        [Fact]
        public async Task CreateCognateSet_AddAttestedExamples_EndToEndTest()
        {
            // Arrange
            var command = new CreateCognateSetCommand
            {
                ProtoForm = "*pater",
                Field = SemanticField.Kinship,
                ReconstructedFromId = _phonemeInventoryId,
                AttestedExamples = new Dictionary<string, string>
                {
                    { "Latin", "pater" },
                    { "Ancient Greek", "patēr" }
                }
            };

            // Act - Create the cognate set
            var createResponse = await _client.PostAsJsonAsync("/api/CognateSets", command);

            // Assert - Creation was successful
            createResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Get the location of the created resource
            var location = createResponse.Headers.Location.ToString();
            var id = location.Substring(location.LastIndexOf('/') + 1);

            // Act - Get the created cognate set
            var getResponse = await _client.GetAsync($"/api/CognateSets/{id}");

            // Assert - Get was successful
            getResponse.EnsureSuccessStatusCode();
            var cognateSet = await getResponse.Content.ReadFromJsonAsync<CognateSetDto>();

            Assert.Equal("*pater", cognateSet.ProtoForm);
            Assert.Equal("Kinship", cognateSet.Field);
            Assert.Equal(2, cognateSet.AttestedExamples.Count);
            Assert.Equal("pater", cognateSet.AttestedExamples["Latin"]);
            Assert.Equal("patēr", cognateSet.AttestedExamples["Ancient Greek"]);

            // Act - Add an attested example
            var addExampleCommand = new AddAttestedExampleCommand
            {
                Language = "Sanskrit",
                Form = "pitā"
            };

            var addExampleResponse = await _client.PostAsJsonAsync($"/api/CognateSets/{id}/attestedExamples", addExampleCommand);

            // Assert - Add example was successful
            Assert.Equal(HttpStatusCode.NoContent, addExampleResponse.StatusCode);

            // Act - Get the updated cognate set
            var getUpdatedResponse = await _client.GetAsync($"/api/CognateSets/{id}");

            // Assert - Get was successful and example was added
            getUpdatedResponse.EnsureSuccessStatusCode();
            var updatedCognateSet = await getUpdatedResponse.Content.ReadFromJsonAsync<CognateSetDto>();

            Assert.Equal(3, updatedCognateSet.AttestedExamples.Count);
            Assert.Equal("pitā", updatedCognateSet.AttestedExamples["Sanskrit"]);

            // Act - Update the cognate set
            var updateCommand = new UpdateCognateSetCommand
            {
                ProtoForm = "*pH₂tḗr",
                Field = SemanticField.Kinship
            };

            var updateResponse = await _client.PatchAsJsonAsync($"/api/CognateSets/{id}", updateCommand);

            // Assert - Update was successful
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // Act - Get the final cognate set
            var getFinalResponse = await _client.GetAsync($"/api/CognateSets/{id}");

            // Assert - Get was successful and updates were applied
            getFinalResponse.EnsureSuccessStatusCode();
            var finalCognateSet = await getFinalResponse.Content.ReadFromJsonAsync<CognateSetDto>();

            Assert.Equal("*pH₂tḗr", finalCognateSet.ProtoForm);
        }
    }

    public class SemanticShiftIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string _authToken;
        private Guid _cognateSetId;

        public SemanticShiftIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            SetupTestDataAsync().Wait();
        }

        private async Task SetupTestDataAsync()
        {
            // Authenticate to get a token
            var loginDto = new LoginDto
            {
                Email = "testresearcher@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            response.EnsureSuccessStatusCode();

            var authResult = await response.Content.ReadFromJsonAsync<AuthResultDto>();
            _authToken = authResult.Token;

            // Set authorization header for subsequent requests
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");

            // Create necessary test data (phoneme inventory and cognate set)

            // Create a phoneme inventory
            var inventoryCommand = new CreatePhonemeInventoryCommand
            {
                Name = "Test-Language",
                StressPattern = StressPattern.Initial,
                Consonants = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "p" },
                    new PhonemeCommand { Value = "t" }
                },
                Vowels = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "a" },
                    new PhonemeCommand { Value = "i" }
                }
            };

            var inventoryResponse = await _client.PostAsJsonAsync("/api/PhonemeInventories", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();

            var inventoryLocation = inventoryResponse.Headers.Location.ToString();
            var inventoryId = Guid.Parse(inventoryLocation.Substring(inventoryLocation.LastIndexOf('/') + 1));

            // Create a cognate set
            var cognateCommand = new CreateCognateSetCommand
            {
                ProtoForm = "*kwekwlos",
                Field = SemanticField.Technology,
                ReconstructedFromId = inventoryId,
                AttestedExamples = new Dictionary<string, string>
                {
                    { "Ancient Greek", "kyklos" },
                    { "Latin", "cyclus" }
                }
            };

            var cognateResponse = await _client.PostAsJsonAsync("/api/CognateSets", cognateCommand);
            cognateResponse.EnsureSuccessStatusCode();

            var cognateLocation = cognateResponse.Headers.Location.ToString();
            _cognateSetId = Guid.Parse(cognateLocation.Substring(cognateLocation.LastIndexOf('/') + 1));
        }

        [Fact]
        public async Task CreateSemanticShift_AndRetrieve_EndToEndTest()
        {
            // Arrange
            var command = new LinguisticsApp.Application.Common.Commands.SemanticShiftCommands.CreateSemanticShiftCommand
            {
                OriginalMeaning = "wheel",
                ModernMeaning = "circle",
                Trigger = ShiftTrigger.Specialization,
                AffectedCognateId = _cognateSetId
            };

            // Act - Create the semantic shift
            var createResponse = await _client.PostAsJsonAsync("/api/SemanticShifts", command);

            // Assert - Creation was successful
            createResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Get the location of the created resource
            var location = createResponse.Headers.Location.ToString();
            var id = location.Substring(location.LastIndexOf('/') + 1);

            // Act - Get the created semantic shift
            var getResponse = await _client.GetAsync($"/api/SemanticShifts/{id}");

            // Assert - Get was successful
            getResponse.EnsureSuccessStatusCode();
            var shift = await getResponse.Content.ReadFromJsonAsync<LinguisticsApp.Application.Common.DTOs.SemanticShiftDTOs.SemanticShiftDto>();

            Assert.Equal("wheel", shift.OriginalMeaning);
            Assert.Equal("circle", shift.ModernMeaning);
            Assert.Equal("Specialization", shift.Trigger);
            Assert.Equal(_cognateSetId, shift.AffectedCognate.Id);

            // Act - Update the semantic shift
            var updateCommand = new LinguisticsApp.Application.Common.Commands.SemanticShiftCommands.UpdateSemanticShiftCommand
            {
                OriginalMeaning = "wheel",
                ModernMeaning = "bicycle", // Changed meaning
                Trigger = ShiftTrigger.TechnologicalChange // Changed trigger
            };

            var updateResponse = await _client.PatchAsJsonAsync($"/api/SemanticShifts/{id}", updateCommand);

            // Assert - Update was successful
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // Act - Get the updated semantic shift
            var getUpdatedResponse = await _client.GetAsync($"/api/SemanticShifts/{id}");

            // Assert - Get was successful and updates were applied
            getUpdatedResponse.EnsureSuccessStatusCode();
            var updatedShift = await getUpdatedResponse.Content.ReadFromJsonAsync<LinguisticsApp.Application.Common.DTOs.SemanticShiftDTOs.SemanticShiftDto>();

            Assert.Equal("wheel", updatedShift.OriginalMeaning);
            Assert.Equal("bicycle", updatedShift.ModernMeaning);
            Assert.Equal("TechnologicalChange", updatedShift.Trigger);
        }
    }

    public class SoundShiftRuleIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string _authToken;
        private Guid _phonemeInventoryId;

        public SoundShiftRuleIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            SetupTestDataAsync().Wait();
        }

        private async Task SetupTestDataAsync()
        {
            // Authenticate to get a token
            var loginDto = new LoginDto
            {
                Email = "testresearcher@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            response.EnsureSuccessStatusCode();

            var authResult = await response.Content.ReadFromJsonAsync<AuthResultDto>();
            _authToken = authResult.Token;

            // Set authorization header for subsequent requests
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");

            // Create a phoneme inventory to use in tests
            var command = new CreatePhonemeInventoryCommand
            {
                Name = "Germanic",
                StressPattern = StressPattern.Initial,
                Consonants = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "p" },
                    new PhonemeCommand { Value = "t" },
                    new PhonemeCommand { Value = "k" },
                    new PhonemeCommand { Value = "b" },
                    new PhonemeCommand { Value = "d" },
                    new PhonemeCommand { Value = "g" }
                },
                Vowels = new List<PhonemeCommand>
                {
                    new PhonemeCommand { Value = "a" },
                    new PhonemeCommand { Value = "e" },
                    new PhonemeCommand { Value = "i" },
                    new PhonemeCommand { Value = "o" },
                    new PhonemeCommand { Value = "u" }
                }
            };

            var createResponse = await _client.PostAsJsonAsync("/api/PhonemeInventories", command);
            createResponse.EnsureSuccessStatusCode();

            var location = createResponse.Headers.Location.ToString();
            _phonemeInventoryId = Guid.Parse(location.Substring(location.LastIndexOf('/') + 1));
        }

        [Fact]
        public async Task CreateSoundShiftRule_AndRetrieve_EndToEndTest()
        {
            // Arrange
            var command = new LinguisticsApp.Application.Common.Commands.SoundShiftRuleCommands.CreateSoundShiftRuleCommand
            {
                Type = RuleType.ConsonantShift,
                Environment = "Word Initial",
                InputPhonemeValue = "p",
                InputPhonemeIsVowel = false,
                OutputPhonemeValue = "f",
                OutputPhonemeIsVowel = false,
                AppliesToId = _phonemeInventoryId
            };

            // Act - Create the sound shift rule
            var createResponse = await _client.PostAsJsonAsync("/api/SoundShiftRules", command);

            // Assert - Creation was successful
            createResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Get the location of the created resource
            var location = createResponse.Headers.Location.ToString();
            var id = location.Substring(location.LastIndexOf('/') + 1);

            // Act - Get the created sound shift rule
            var getResponse = await _client.GetAsync($"/api/SoundShiftRules/{id}");

            // Assert - Get was successful
            getResponse.EnsureSuccessStatusCode();
            var rule = await getResponse.Content.ReadFromJsonAsync<LinguisticsApp.Application.Common.DTOs.SoundShiftRuleDTOs.SoundShiftRuleDto>();

            Assert.Equal("ConsonantShift", rule.Type);
            Assert.Equal("Word Initial", rule.Environment);
            Assert.Equal("p", rule.InputPhoneme.Value);
            Assert.False(rule.InputPhoneme.IsVowel);
            Assert.Equal("f", rule.OutputPhoneme.Value);
            Assert.False(rule.OutputPhoneme.IsVowel);
            Assert.Equal(_phonemeInventoryId, rule.AppliesTo.Id);

            // Act - Update the sound shift rule
            var updateCommand = new LinguisticsApp.Application.Common.Commands.SoundShiftRuleCommands.UpdateSoundShiftRuleCommand
            {
                Type = RuleType.ConsonantShift,
                Environment = "All Positions", // Changed environment
                InputPhonemeValue = "p",
                InputPhonemeIsVowel = false,
                OutputPhonemeValue = "pf", // Changed output
                OutputPhonemeIsVowel = false
            };

            var updateResponse = await _client.PatchAsJsonAsync($"/api/SoundShiftRules/{id}", updateCommand);

            // Assert - Update was successful
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // Act - Get the updated sound shift rule
            var getUpdatedResponse = await _client.GetAsync($"/api/SoundShiftRules/{id}");

            // Assert - Get was successful and updates were applied
            getUpdatedResponse.EnsureSuccessStatusCode();
            var updatedRule = await getUpdatedResponse.Content.ReadFromJsonAsync<LinguisticsApp.Application.Common.DTOs.SoundShiftRuleDTOs.SoundShiftRuleDto>();

            Assert.Equal("ConsonantShift", updatedRule.Type);
            Assert.Equal("All Positions", updatedRule.Environment);
            Assert.Equal("p", updatedRule.InputPhoneme.Value);
            Assert.Equal("pf", updatedRule.OutputPhoneme.Value);
        }

        [Fact]
        public async Task GetSoundShiftRulesByPhonemeInventory_ReturnsCorrectRules()
        {
            // Arrange - Create two sound shift rules for the same phoneme inventory
            var command1 = new LinguisticsApp.Application.Common.Commands.SoundShiftRuleCommands.CreateSoundShiftRuleCommand
            {
                Type = RuleType.ConsonantShift,
                Environment = "Word Initial",
                InputPhonemeValue = "p",
                InputPhonemeIsVowel = false,
                OutputPhonemeValue = "f",
                OutputPhonemeIsVowel = false,
                AppliesToId = _phonemeInventoryId
            };

            var command2 = new LinguisticsApp.Application.Common.Commands.SoundShiftRuleCommands.CreateSoundShiftRuleCommand
            {
                Type = RuleType.ConsonantShift,
                Environment = "Word Initial",
                InputPhonemeValue = "t",
                InputPhonemeIsVowel = false,
                OutputPhonemeValue = "th",
                OutputPhonemeIsVowel = false,
                AppliesToId = _phonemeInventoryId
            };

            await _client.PostAsJsonAsync("/api/SoundShiftRules", command1);
            await _client.PostAsJsonAsync("/api/SoundShiftRules", command2);

            // Act - Get rules by phoneme inventory
            var response = await _client.GetAsync($"/api/SoundShiftRules/byPhonemeInventory/{_phonemeInventoryId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var rules = await response.Content.ReadFromJsonAsync<List<LinguisticsApp.Application.Common.DTOs.SoundShiftRuleDTOs.SoundShiftRuleDto>>();

            Assert.True(rules.Count >= 2);
            Assert.Contains(rules, r => r.InputPhoneme.Value == "p" && r.OutputPhoneme.Value == "f");
            Assert.Contains(rules, r => r.InputPhoneme.Value == "t" && r.OutputPhoneme.Value == "th");
        }
    }

    // Extension method for PATCH requests
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            var content = JsonContent.Create(value);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return client.SendAsync(request);
        }
    }

    // Custom WebApplicationFactory for testing
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Find the descriptor for the DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<LinguisticsDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using an in-memory database for testing
                services.AddDbContext<LinguisticsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<LinguisticsDbContext>();
                    var authService = scopedServices.GetRequiredService<LinguisticsApp.Application.Common.Interfaces.Services.IAuthService>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    // Seed the database with test data
                    SeedTestData(db, authService);
                }
            });
        }

        private void SeedTestData(LinguisticsDbContext db, LinguisticsApp.Application.Common.Interfaces.Services.IAuthService authService)
        {
            // Add a test researcher
            var researcher = new Researcher(
                UserId.New(),
                new Email("testresearcher@example.com"),
                authService.HashPassword("Password123!"),
                "Test",
                "Researcher",
                "University of Testing",
                ResearchField.HistoricalLinguistics
            );

            db.Researchers.Add(researcher);
            db.SaveChanges();
        }
    }
}