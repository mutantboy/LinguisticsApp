using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.ValueObjects;
using LinguisticsApp.DomainModel;

namespace LinguisticsApp.Infrastructure.Persistence
{
    public class LinguisticsDbContext : DbContext
    {
        public DbSet<Researcher> Researchers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<PhonemeInventory> PhonemeInventories { get; set; }
        public DbSet<SoundShiftRule> SoundShiftRules { get; set; }
        public DbSet<CognateSet> CognateSets { get; set; }
        public DbSet<SemanticShift> SemanticShifts { get; set; }
        public DbSet<LanguageContactEvent> ContactEvents { get; set; }

        private static readonly string[] _stressPatternValues = ["I", "P", "U", "F", "L"];
        private static readonly string[] _ruleTypeValues = ["CS", "VM", "M", "E", "D"];
        private static readonly string[] _semanticFieldValues = ["K", "N", "T", "R", "E"];
        private static readonly string[] _shiftTriggerValues = ["CT", "TC", "SC", "E", "S"];
        private static readonly string[] _contactTypeValues = ["T", "C", "R", "M", "CQ"];
        private static readonly string[] _researchFieldValues = ["HL", "CL", "P", "S", "CS"];

        public LinguisticsDbContext(DbContextOptions<LinguisticsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add all custom value converters
            modelBuilder.AddStronglyTypedIdValueConverters();

            //enum conversions
            modelBuilder.Entity<PhonemeInventory>()
                .Property(pi => pi.StressPattern)
                .HasConversion(
                    v => _stressPatternValues[(int)v],
                    v => (StressPattern)Array.IndexOf(_stressPatternValues, v));

            modelBuilder.Entity<SoundShiftRule>()
                .Property(ssr => ssr.Type)
                .HasConversion(
                    v => _ruleTypeValues[(int)v],
                    v => (RuleType)Array.IndexOf(_ruleTypeValues, v));

            modelBuilder.Entity<CognateSet>()
                .Property(cs => cs.Field)
                .HasConversion(
                    v => _semanticFieldValues[(int)v],
                    v => (SemanticField)Array.IndexOf(_semanticFieldValues, v));

            modelBuilder.Entity<SemanticShift>()
                .Property(ss => ss.Trigger)
                .HasConversion(
                    v => _shiftTriggerValues[(int)v],
                    v => (ShiftTrigger)Array.IndexOf(_shiftTriggerValues, v));

            modelBuilder.Entity<LanguageContactEvent>()
                .Property(lce => lce.Type)
                .HasConversion(
                    v => _contactTypeValues[(int)v],
                    v => (ContactType)Array.IndexOf(_contactTypeValues, v));

            modelBuilder.Entity<Researcher>()
                .Property(r => r.Field)
                .HasConversion(
                    v => _researchFieldValues[(int)v],
                    v => (ResearchField)Array.IndexOf(_researchFieldValues, v));

            // value object stuff
            modelBuilder.Entity<PhonemeInventory>()
                .OwnsMany(pi => pi.Consonants, pb =>
                {
                    pb.ToTable("PhonemeInventoryConsonants");
                    pb.WithOwner().HasForeignKey("PhonemeInventoryId");
                    pb.Property<int>("Id").ValueGeneratedOnAdd();
                    pb.HasKey("Id");
                });

            modelBuilder.Entity<PhonemeInventory>()
                .OwnsMany(pi => pi.Vowels, pb =>
                {
                    pb.ToTable("PhonemeInventoryVowels");
                    pb.WithOwner().HasForeignKey("PhonemeInventoryId");
                    pb.Property<int>("Id").ValueGeneratedOnAdd();
                    pb.HasKey("Id");
                });

            modelBuilder.Entity<CognateSet>()
                .Property(cs => cs.AttestedExamples)
                .HasConversion(
                    v => SerializeAttestedExamples(v),
                    v => DeserializeAttestedExamples(v));

            modelBuilder.Entity<LanguageContactEvent>()
                .Property(lce => lce.LoanwordsAdopted)
                .HasConversion(
                    v => string.Join(";", v),
                    v => new HashSet<string>(v.Split(';', StringSplitOptions.RemoveEmptyEntries)));

            // Configure relationships
            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasDiscriminator<string>("UserType")
                .HasValue<Researcher>("Researcher")
                .HasValue<Admin>("Admin");

            modelBuilder.Entity<SoundShiftRule>()
                .HasOne(ssr => ssr.AppliesTo)
                .WithMany(pi => pi.SoundShiftRules)
                .HasForeignKey("AppliesToId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SoundShiftRule>()
                .HasOne(ssr => ssr.Creator)
                .WithMany()
                .HasForeignKey("CreatorId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CognateSet>()
                .HasOne(cs => cs.ReconstructedFrom)
                .WithMany(pi => pi.CognateSets)
                .HasForeignKey("ReconstructedFromId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SemanticShift>()
                .HasOne(ss => ss.AffectedCognate)
                .WithMany(cs => cs.SemanticShifts)
                .HasForeignKey("AffectedCognateId")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SemanticShift>()
                .HasMany(ss => ss.RelatedEvents)
                .WithMany(lce => lce.CausedShifts)
                .UsingEntity(j => j.ToTable("SemanticShiftEvents"));

            modelBuilder.Entity<PhonemeInventory>()
                .HasOne(pi => pi.Creator)
                .WithMany()
                .HasForeignKey("CreatorId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LanguageContactEvent>()
                .HasOne(lce => lce.SourceLanguage)
                .WithMany()
                .HasForeignKey("SourceLanguageId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LanguageContactEvent>()
                .HasOne(lce => lce.TargetLanguage)
                .WithMany()
                .HasForeignKey("TargetLanguageId")
                .OnDelete(DeleteBehavior.Restrict);

            // Configure concurrency tokens
            modelBuilder.Entity<PhonemeInventory>()
                .Property<byte[]>("RowVersion")
                .IsRowVersion();

            modelBuilder.Entity<SoundShiftRule>()
                .Property<byte[]>("RowVersion")
                .IsRowVersion();

            modelBuilder.Entity<CognateSet>()
                .Property<byte[]>("RowVersion")
                .IsRowVersion();
        }

        // Helper methods for converting Dictionary to string and back
        private string SerializeAttestedExamples(Dictionary<string, string> examples)
        {
            return string.Join("|", examples.Select(kv => $"{kv.Key}:{kv.Value}"));
        }

        private Dictionary<string, string> DeserializeAttestedExamples(string serialized)
        {
            if (string.IsNullOrEmpty(serialized))
                return new Dictionary<string, string>();

            return serialized
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(':', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0], parts => parts[1]);
        }
    }

    // Extensions for ModelBuilder to automatically register all value converters
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder AddStronglyTypedIdValueConverters(this ModelBuilder modelBuilder)
        {
            var assembly = typeof(EfCoreValueConverterAttribute).Assembly;

            // Find all types with the attribute
            var typesWithConverterAttribute = assembly.GetTypes()
                .Where(t => t.GetCustomAttributes<EfCoreValueConverterAttribute>().Any());

            foreach (var type in typesWithConverterAttribute)
            {
                var attribute = type.GetCustomAttribute<EfCoreValueConverterAttribute>();
                if (attribute == null) continue;

                var converter = Activator.CreateInstance(attribute.ValueConverter) as ValueConverter;
                if (converter == null) continue;

                modelBuilder.ApplyValueConverterToMatchingProperties(converter);
            }

            return modelBuilder;
        }

        public static ModelBuilder ApplyValueConverterToMatchingProperties(
            this ModelBuilder modelBuilder, ValueConverter converter)
        {
            var type = converter.ModelClrType;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == type);

                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(converter);
                }
            }

            return modelBuilder;
        }
    }
}