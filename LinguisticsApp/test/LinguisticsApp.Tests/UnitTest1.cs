using Xunit;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.ValueObjects;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.Enumerations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using LinguisticsApp.Infrastructure.Persistence;

namespace LinguisticsApp.Tests.UnitTests
{
    public class ValueObjectTests
    {
        [Fact]
        public void Phoneme_EqualityTest()
        {
            // Arrange
            var phoneme1 = new Phoneme("p", false);
            var phoneme2 = new Phoneme("p", false);
            var phoneme3 = new Phoneme("p", true);
            var phoneme4 = new Phoneme("b", false);

            // Act & Assert
            Assert.Equal(phoneme1, phoneme2);
            Assert.NotEqual(phoneme1, phoneme3);
            Assert.NotEqual(phoneme1, phoneme4);
        }

        [Fact]
        public void Phoneme_WithEmptyValue_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Phoneme("", false));
            Assert.Throws<ArgumentException>(() => new Phoneme(" ", true));
        }
    }

    public class RichTypeTests
    {
        [Fact]
        public void Email_ValidEmail_CreatesInstance()
        {
            // Arrange & Act
            var email = new Email("test@example.com");

            // Assert
            Assert.Equal("test@example.com", email.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid")]
        [InlineData("invalid@")]
        [InlineData("@example.com")]
        public void Email_InvalidEmail_ThrowsArgumentException(string invalidEmail)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
        }

        [Fact]
        public void LanguageName_ValidName_CreatesInstance()
        {
            // Arrange & Act
            var languageName = new LanguageName("English");

            // Assert
            Assert.Equal("English", languageName.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void LanguageName_InvalidName_ThrowsArgumentException(string invalidName)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new LanguageName(invalidName));
        }

        [Fact]
        public void ProtoForm_ValidValue_CreatesInstance()
        {
            // Arrange & Act
            var protoForm = new ProtoForm("*wod-");

            // Assert
            Assert.Equal("*wod-", protoForm.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ProtoForm_InvalidValue_ThrowsArgumentException(string invalidValue)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ProtoForm(invalidValue));
        }

        [Fact]
        public void UserId_NewCreatesUniqueIdentifiers()
        {
            // Arrange & Act
            var id1 = UserId.New();
            var id2 = UserId.New();

            // Assert
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void UserId_SameValueEqualsReturnsTrue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var id1 = new UserId(guid);
            var id2 = new UserId(guid);

            // Act & Assert
            Assert.Equal(id1, id2);
        }
    }

    public class EntityTests
    {
        [Fact]
        public void PhonemeInventory_AddConsonant_AddsToCollection()
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var inventory = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Test Language"),
                StressPattern.Initial,
                researcher
            );

            var consonant = new Phoneme("p", false);

            // Act
            inventory.AddConsonant(consonant);

            // Assert
            Assert.Contains(consonant, inventory.Consonants);
        }

        [Fact]
        public void PhonemeInventory_AddVowel_AddsToCollection()
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var inventory = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Test Language"),
                StressPattern.Initial,
                researcher
            );

            var vowel = new Phoneme("a", true);

            // Act
            inventory.AddVowel(vowel);

            // Assert
            Assert.Contains(vowel, inventory.Vowels);
        }

        [Fact]
        public void PhonemeInventory_AddVowelAsConsonant_ThrowsArgumentException()
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var inventory = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Test Language"),
                StressPattern.Initial,
                researcher
            );

            var vowel = new Phoneme("a", true);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => inventory.AddConsonant(vowel));
        }

        [Fact]
        public void PhonemeInventory_AddConsonantAsVowel_ThrowsArgumentException()
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var inventory = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Test Language"),
                StressPattern.Initial,
                researcher
            );

            var consonant = new Phoneme("p", false);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => inventory.AddVowel(consonant));
        }

        [Fact]
        public void CognateSet_AddAttestedExample_AddsToCollection()
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var inventory = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Test Language"),
                StressPattern.Initial,
                researcher
            );

            var cognateSet = new CognateSet(
                CognateSetId.New(),
                new ProtoForm("*wod-"),
                SemanticField.Kinship,
                inventory
            );

            // Act
            cognateSet.AddAttestedExample("English", "water");

            // Assert
            Assert.Equal("water", cognateSet.AttestedExamples["English"]);
        }

        [Theory]
        [InlineData("", "water")]
        [InlineData("English", "")]
        public void CognateSet_AddAttestedExampleWithEmptyValues_ThrowsArgumentException(string language, string form)
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            var inventory = new PhonemeInventory(
                PhonemeInventoryId.New(),
                new LanguageName("Test Language"),
                StressPattern.Initial,
                researcher
            );

            var cognateSet = new CognateSet(
                CognateSetId.New(),
                new ProtoForm("*wod-"),
                SemanticField.Kinship,
                inventory
            );

            // Act & Assert
            Assert.Throws<ArgumentException>(() => cognateSet.AddAttestedExample(language, form));
        }

        [Fact]
        public void LanguageContactEvent_AddLoanword_AddsToCollection()
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
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
                LanguageContactEventId.New(),
                ContactType.Trade,
                "Grammar influence",
                sourceLanguage,
                targetLanguage
            );

            // Act
            contactEvent.AddLoanword("borrowed word");

            // Assert
            Assert.Contains("borrowed word", contactEvent.LoanwordsAdopted);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void LanguageContactEvent_AddEmptyLoanword_ThrowsArgumentException(string emptyLoanword)
        {
            // Arrange
            var researcher = new Researcher(
                UserId.New(),
                new Email("researcher@example.com"),
                "password",
                "John",
                "Doe",
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
                LanguageContactEventId.New(),
                ContactType.Trade,
                "Grammar influence",
                sourceLanguage,
                targetLanguage
            );

            // Act & Assert
            Assert.Throws<ArgumentException>(() => contactEvent.AddLoanword(emptyLoanword));
        }
    }

    public class DbContextTests
    {
        private LinguisticsDbContext GetDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<LinguisticsDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new LinguisticsDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public void DatabaseCreation_Success()
        {
            // Arrange & Act
            using var context = GetDbContext();

            // Assert
            Assert.NotNull(context);
            // Ensure tables exist by checking if we can query them
            Assert.Empty(context.PhonemeInventories);
            Assert.Empty(context.Researchers);
            Assert.Empty(context.Admins);
        }

        [Fact]
        public void AddUser_RetrieveById_Success()
        {
            // Arrange
            using var context = GetDbContext();
            var userId = UserId.New();
            var admin = new Admin(
                userId,
                new Email("admin@example.com"),
                "password123",
                "Admin",
                "User",
                true
            );

            // Act
            context.Admins.Add(admin);
            context.SaveChanges();

            context.ChangeTracker.Clear(); // Clear tracked entities

            var retrievedAdmin = context.Admins.FirstOrDefault(a => a.Id == userId);

            // Assert
            Assert.NotNull(retrievedAdmin);
            Assert.Equal("admin@example.com", retrievedAdmin.Username.Value);
            Assert.Equal("Admin", retrievedAdmin.FirstName);
            Assert.Equal("User", retrievedAdmin.LastName);
            Assert.True(retrievedAdmin.CanModifyRules);
        }

        [Fact]
        public void AddPhonemeInventory_WithPhonemesAndRetrieve_Success()
        {
            // Arrange
            using var context = GetDbContext();

            var userId = UserId.New();
            var researcher = new Researcher(
                userId,
                new Email("researcher@example.com"),
                "password123",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            context.Researchers.Add(researcher);
            context.SaveChanges();

            var inventoryId = PhonemeInventoryId.New();
            var phonemeInventory = new PhonemeInventory(
                inventoryId,
                new LanguageName("English"),
                StressPattern.Free,
                researcher
            );

            phonemeInventory.AddConsonant(new Phoneme("p", false));
            phonemeInventory.AddVowel(new Phoneme("a", true));

            // Act
            context.PhonemeInventories.Add(phonemeInventory);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            var retrievedInventory = context.PhonemeInventories
                .Include(pi => pi.Consonants)
                .Include(pi => pi.Vowels)
                .FirstOrDefault(pi => pi.Id == inventoryId);

            // Assert
            Assert.NotNull(retrievedInventory);
            Assert.Equal("English", retrievedInventory.Name.Value);
            Assert.Equal(StressPattern.Free, retrievedInventory.StressPattern);
            Assert.Single(retrievedInventory.Consonants);
            Assert.Single(retrievedInventory.Vowels);
            Assert.Contains(retrievedInventory.Consonants, p => p.Value == "p");
            Assert.Contains(retrievedInventory.Vowels, p => p.Value == "a");
        }

        [Fact]
        public void AddCognateSet_WithAttestedExamplesAndRetrieve_Success()
        {
            // Arrange
            using var context = GetDbContext();

            var userId = UserId.New();
            var researcher = new Researcher(
                userId,
                new Email("researcher@example.com"),
                "password123",
                "John",
                "Doe",
                "University",
                ResearchField.HistoricalLinguistics
            );

            context.Researchers.Add(researcher);

            var inventoryId = PhonemeInventoryId.New();
            var phonemeInventory = new PhonemeInventory(
                inventoryId,
                new LanguageName("Proto-Indo-European"),
                StressPattern.Free,
                researcher
            );

            context.PhonemeInventories.Add(phonemeInventory);
            context.SaveChanges();

            var cognateSetId = CognateSetId.New();
            var cognateSet = new CognateSet(
                cognateSetId,
                new ProtoForm("*wódr̥"),
                SemanticField.Nature,
                phonemeInventory
            );

            cognateSet.AddAttestedExample("English", "water");
            cognateSet.AddAttestedExample("German", "Wasser");
            cognateSet.AddAttestedExample("Latin", "aqua");

            // Act
            context.CognateSets.Add(cognateSet);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            var retrievedCognateSet = context.CognateSets
                .FirstOrDefault(cs => cs.Id == cognateSetId);

            // Assert
            Assert.NotNull(retrievedCognateSet);
            Assert.Equal("*wódr̥", retrievedCognateSet.ProtoForm.Value);
            Assert.Equal(SemanticField.Nature, retrievedCognateSet.Field);
            Assert.Equal(3, retrievedCognateSet.AttestedExamples.Count);
            Assert.Equal("water", retrievedCognateSet.AttestedExamples["English"]);
            Assert.Equal("Wasser", retrievedCognateSet.AttestedExamples["German"]);
            Assert.Equal("aqua", retrievedCognateSet.AttestedExamples["Latin"]);
        }
    }
}