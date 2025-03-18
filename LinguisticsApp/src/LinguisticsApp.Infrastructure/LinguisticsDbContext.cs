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
            modelBuilder.Entity<User>().Property(u => u.Id).HasConversion(
                id => id.Value,
                value => new UserId(value));

            modelBuilder.Entity<Admin>().Property(u => u.Id).HasConversion(
                id => id.Value,
                value => new UserId(value));

            modelBuilder.Entity<Researcher>().Property(u => u.Id).HasConversion(
                id => id.Value,
                value => new UserId(value));

            // Email rich type
            modelBuilder.Entity<User>()
                .Property(p => p.Username)
                .HasConversion(
                    objValue => objValue.Value,  
                    dbValue => new Email(dbValue) 
                );

            // PhonemeInventory ID
            modelBuilder.Entity<PhonemeInventory>().Property(p => p.Id).HasConversion(
                id => id.Value,
                value => new PhonemeInventoryId(value));

            // Language Name
            modelBuilder.Entity<PhonemeInventory>().Property(p => p.Name).HasConversion(
                name => name.Value,
                value => new LanguageName(value));

            // SoundShiftRule ID
            modelBuilder.Entity<SoundShiftRule>().Property(s => s.Id).HasConversion(
                id => id.Value,
                value => new SoundShiftRuleId(value));

            // CognateSet ID
            modelBuilder.Entity<CognateSet>().Property(c => c.Id).HasConversion(
                id => id.Value,
                value => new CognateSetId(value));

            // ProtoForm
            modelBuilder.Entity<CognateSet>().Property(c => c.ProtoForm).HasConversion(
                form => form.Value,
                value => new ProtoForm(value));

            // SemanticShift ID
            modelBuilder.Entity<SemanticShift>().Property(s => s.Id).HasConversion(
                id => id.Value,
                value => new SemanticShiftId(value));

            // LanguageContactEvent ID
            modelBuilder.Entity<LanguageContactEvent>().Property(l => l.Id).HasConversion(
                id => id.Value,
                value => new LanguageContactEventId(value));

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

            // value object stuff
            modelBuilder.Entity<PhonemeInventory>()
                .OwnsMany(pi => pi.Consonants, builder =>
                {
                    builder.ToTable("PhonemeInventoryConsonants");
                    builder.WithOwner().HasForeignKey("PhonemeInventoryId");
                    builder.Property<int>("Id").ValueGeneratedOnAdd();
                    builder.HasKey("Id");
                    builder.Property(p => p.Value).IsRequired();
                    builder.Property(p => p.IsVowel).IsRequired();
                });

            modelBuilder.Entity<PhonemeInventory>()
                .OwnsMany(pi => pi.Vowels, builder =>
                {
                    builder.ToTable("PhonemeInventoryVowels");
                    builder.WithOwner().HasForeignKey("PhonemeInventoryId");
                    builder.Property<int>("Id").ValueGeneratedOnAdd();
                    builder.HasKey("Id");
                    builder.Property(p => p.Value).IsRequired();
                    builder.Property(p => p.IsVowel).IsRequired();
                });
            modelBuilder.Entity<SoundShiftRule>()
                .OwnsOne(r => r.InputPhoneme, phoneme =>
                {
                    phoneme.ToTable("SoundShiftRuleInputPhonemes");
                    phoneme.Property(p => p.Value).HasColumnName("InputPhonemeValue");
                    phoneme.Property(p => p.IsVowel).HasColumnName("InputPhonemeIsVowel");
                });

            modelBuilder.Entity<SoundShiftRule>()
                .OwnsOne(r => r.OutputPhoneme, phoneme =>
                {
                    phoneme.ToTable("SoundShiftRuleOutputPhonemes");
                    phoneme.Property(p => p.Value).HasColumnName("OutputPhonemeValue");
                    phoneme.Property(p => p.IsVowel).HasColumnName("OutputPhonemeIsVowel");
                });

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

    public static class ModelBuilderExtensions
    {
        public static ModelBuilder AddStronglyTypedIdValueConverters(this ModelBuilder modelBuilder)
        {
            var assembly = typeof(EfCoreValueConverterAttribute).Assembly;

            var typesWithConverterAttribute = assembly.GetTypes()
                .Where(t => t.GetCustomAttributes<EfCoreValueConverterAttribute>().Any());

            foreach (var type in typesWithConverterAttribute)
            {
                var attribute = type.GetCustomAttribute<EfCoreValueConverterAttribute>();
                if (attribute == null) continue;

                try
                {
                    var converter = Activator.CreateInstance(attribute.ValueConverter) as ValueConverter;
                    if (converter == null) continue;

                    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                    {
                        var properties = entityType.GetProperties()
                            .Where(p => p.ClrType == type);

                        foreach (var property in properties)
                        {
                            property.SetValueConverter(converter);
                        }

                        foreach (var navigation in entityType.GetNavigations())
                        {
                            if (navigation.ClrType == type)
                            {
                                navigation.ForeignKey.Properties
                                    .ToList()
                                    .ForEach(p => p.SetValueConverter(converter));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying converter for type {type.Name}: {ex.Message}");
                }
            }

            return modelBuilder;
        }
    }

    // Extensions for ModelBuilder to automatically register all value converters
    /*public static class ModelBuilderExtensions
    {
        public static ModelBuilder AddStronglyTypedIdValueConverters<T>(
            this ModelBuilder modelBuilder)
        {
            var assembly = typeof(T).Assembly;
            foreach (var type in assembly.GetTypes())
            {
                // Try and get the attribute
                var attribute = type
                    .GetCustomAttributes<EfCoreValueConverterAttribute>()
                    .FirstOrDefault();

                if (attribute is null)
                {
                    continue;
                }

                // The ValueConverter must have a parameterless constructor
                var converter = (ValueConverter)Activator.CreateInstance(attribute.ValueConverter);

                // Register the value converter for all EF Core properties that use the ID
                modelBuilder.UseValueConverter(converter);
            }

            return modelBuilder;
        }

        // This method is the same as shown previously
        public static ModelBuilder UseValueConverter(
            this ModelBuilder modelBuilder, ValueConverter converter)
        {
            var type = converter.ModelClrType;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType
                    .ClrType
                    .GetProperties()
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

        public static ModelBuilder ApplyValueConverterToMatchingProperties(this ModelBuilder modelBuilder, ValueConverter converter)
        {
            var type = converter.ModelClrType;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Try to find properties directly on the entity type
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == type)
                    {
                        property.SetValueConverter(converter);
                    }
                }
            }

            return modelBuilder;
        }
    }*/
}