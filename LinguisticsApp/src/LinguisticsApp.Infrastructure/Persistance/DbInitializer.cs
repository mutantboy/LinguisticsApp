using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.ValueObjects;
using LinguisticsApp.Infrastructure.Persistence;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.Application.Common.Interfaces.Services;

namespace LinguisticsApp.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider, bool useSampleData = true)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LinguisticsDbContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            dbContext.Database.EnsureCreated();

            if (dbContext.Admins.Any() || dbContext.Researchers.Any())
            {
                return;
            }

            var admin = new Admin(
                UserId.New(),
                new Email("trojan.brbojevic@loewe.com"),
                authService.HashPassword("Admin123!"),
                "Trojan",
                "Brbojevic",
                true
            );

            dbContext.Admins.Add(admin);

            if (useSampleData)
            {
                SeedSampleData(dbContext, authService);
            }

            dbContext.SaveChanges();
        }

        private static void SeedSampleData(LinguisticsDbContext dbContext, IAuthService authService)
        {
            var faker = new Faker();

            var researchers = new Faker<Researcher>()
                .CustomInstantiator(f => new Researcher(
                    UserId.New(),
                    new Email(f.Internet.Email(uniqueSuffix: f.Random.Number(1000, 9999).ToString())),
                    authService.HashPassword("Password123!"),
                    f.Name.FirstName(),
                    f.Name.LastName(),
                    f.Company.CompanyName(),
                    f.PickRandom<ResearchField>()
                ))
                .Generate(5);

            dbContext.Researchers.AddRange(researchers);

            var languageFamilies = new List<string>
            {
                "Indo-European", "Uralic", "Altaic", "Sino-Tibetan", "Afro-Asiatic"
            };

            var phonemeInventories = new List<PhonemeInventory>();

            foreach (var family in languageFamilies)
            {
                var baseName = $"Proto-{family}";
                var inventory = new PhonemeInventory(
                    PhonemeInventoryId.New(),
                    new LanguageName(baseName),
                    faker.PickRandom<StressPattern>(),
                    faker.PickRandom(researchers)
                );

                var consonants = new List<string> { "p", "t", "k", "b", "d", "g", "m", "n", "s", "r", "l" };
                foreach (var c in faker.Random.ListItems(consonants, faker.Random.Int(6, consonants.Count)))
                {
                    inventory.AddConsonant(new Phoneme(c, false));
                }

                var vowels = new List<string> { "a", "e", "i", "o", "u" };
                foreach (var v in faker.Random.ListItems(vowels, faker.Random.Int(3, vowels.Count)))
                {
                    inventory.AddVowel(new Phoneme(v, true));
                }

                phonemeInventories.Add(inventory);

                var descendants = new Faker<PhonemeInventory>()
                    .CustomInstantiator(f => new PhonemeInventory(
                        PhonemeInventoryId.New(),
                        new LanguageName(f.Random.ListItem(new[]
                        {
                            $"Old {baseName.Substring(6)}",
                            $"Classical {baseName.Substring(6)}",
                            $"Archaic {baseName.Substring(6)}"
                        })),
                        f.PickRandom<StressPattern>(),
                        f.PickRandom(researchers)
                    ))
                    .Generate(2);

                foreach (var descendant in descendants)
                {
                    /// Copy consonants and vowels from parent with some variations
                    foreach (var c in inventory.Consonants)
                    {
                        if (faker.Random.Bool(0.9f)) /// 90% chance of keeping the consonant
                        {
                            descendant.AddConsonant(c);
                        }
                    }

                    /// unique consonants
                    if (faker.Random.Bool(0.5f))
                    {
                        var extraConsonants = new List<string> { "f", "v", "z", "h", "j", "w" };
                        var extra = faker.Random.ListItem(extraConsonants);
                        if (!descendant.Consonants.Any(c => c.Value == extra))
                        {
                            descendant.AddConsonant(new Phoneme(extra, false));
                        }
                    }

                    /// Copy vowels
                    foreach (var v in inventory.Vowels)
                    {
                        if (faker.Random.Bool(0.9f)) /// 90% chance of keeping the vowel
                        {
                            descendant.AddVowel(v);
                        }
                    }

                    phonemeInventories.Add(descendant);
                }
            }

            dbContext.PhonemeInventories.AddRange(phonemeInventories);

            var soundShiftRules = new List<SoundShiftRule>();

            foreach (var descendant in phonemeInventories.Where(pi => !pi.Name.Value.StartsWith("Proto-")))
            {
                var parent = phonemeInventories.FirstOrDefault(pi =>
                    pi.Name.Value.StartsWith("Proto-") &&
                    descendant.Name.Value.Contains(pi.Name.Value.Substring(6)));

                if (parent != null)
                {
                    // Create 2-3 sound shift rules for each descendant language
                    var numRules = faker.Random.Int(2, 3);
                    for (int i = 0; i < numRules; i++)
                    {
                        if (parent.Consonants.Any() && descendant.Consonants.Any())
                        {
                            var inputPhoneme = faker.Random.ListItem(parent.Consonants.ToList());
                            var outputPhoneme = faker.Random.ListItem(descendant.Consonants.ToList());

                            var rule = new SoundShiftRule(
                                SoundShiftRuleId.New(),
                                faker.PickRandom<RuleType>(),
                                faker.Random.ListItem(new[] { "Word Initial", "Intervocalic", "Word Final", "Before Consonant" }),
                                inputPhoneme,
                                outputPhoneme,
                                descendant,
                                faker.PickRandom(researchers)
                            );

                            soundShiftRules.Add(rule);
                        }
                    }
                }
            }

            dbContext.SoundShiftRules.AddRange(soundShiftRules);

            var cognateSets = new List<CognateSet>();

            var protoForms = new List<string>
            {
                "*ph₂tḗr", "*méh₂tēr", "*bʰréh₂tēr", "*swésōr", "*déḱm̥", "*wĺ̥kʷos", "*ḱm̥tóm"
            };

            var meanings = new List<string>
            {
                "father", "mother", "brother", "sister", "ten", "wolf", "hundred"
            };

            for (int i = 0; i < protoForms.Count; i++)
            {
                var protoInventory = phonemeInventories.FirstOrDefault(pi => pi.Name.Value.StartsWith("Proto-"));
                if (protoInventory != null)
                {
                    var cognateSet = new CognateSet(
                        CognateSetId.New(),
                        new ProtoForm(protoForms[i]),
                        faker.PickRandom<SemanticField>(),
                        protoInventory
                    );

                    foreach (var inventory in phonemeInventories.Where(pi => !pi.Name.Value.StartsWith("Proto-")))
                    {
                        var derivedForm = protoForms[i]
                            .Replace("*", "")
                            .Replace("h₂", faker.Random.Bool() ? "a" : "")
                            .Replace("ḗ", "ē")
                            .Replace("ḱ", faker.Random.Bool() ? "k" : "s")
                            .Replace("ḱ", "k")
                            .Replace("m̥", "m")
                            .Replace("ĺ̥", "l")
                            .Replace("kʷ", faker.Random.Bool() ? "k" : "p");

                        cognateSet.AddAttestedExample(inventory.Name.Value, derivedForm);
                    }

                    cognateSets.Add(cognateSet);

                    var semanticShift = new SemanticShift(
                        SemanticShiftId.New(),
                        meanings[i],
                        faker.Random.Bool(0.3f) ? faker.PickRandom(new[]
                        {
                            "extended meaning",
                            "specialized meaning",
                            "metaphorical usage"
                        }) + " of " + meanings[i] : meanings[i],
                        faker.PickRandom<ShiftTrigger>(),
                        cognateSet
                    );

                    dbContext.SemanticShifts.Add(semanticShift);
                }
            }

            dbContext.CognateSets.AddRange(cognateSets);

            var contactEvents = new List<LanguageContactEvent>();

            ///  3 random contact events
            for (int i = 0; i < 3; i++)
            {
                var sourceLanguage = faker.Random.ListItem(phonemeInventories);
                var targetLanguage = faker.Random.ListItem(phonemeInventories.Where(pi => pi != sourceLanguage).ToList());

                var contactEvent = new LanguageContactEvent(
                    LanguageContactEventId.New(),
                    faker.PickRandom<ContactType>(),
                    faker.Lorem.Sentence(),
                    sourceLanguage,
                    targetLanguage
                );

                /// loanwords
                var numLoans = faker.Random.Int(2, 5);
                for (int j = 0; j < numLoans; j++)
                {
                    contactEvent.AddLoanword(faker.Lorem.Word());
                }

                contactEvents.Add(contactEvent);
            }

            dbContext.ContactEvents.AddRange(contactEvents);
        }
    }
}