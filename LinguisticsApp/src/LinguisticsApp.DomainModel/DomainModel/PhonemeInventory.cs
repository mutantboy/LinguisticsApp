using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.DomainModel
{
    public class PhonemeInventory : Entity<PhonemeInventoryId>
    {
        public LanguageName Name { get; private set; }
        public HashSet<Phoneme> Consonants { get; private set; } = new HashSet<Phoneme>();
        public HashSet<Phoneme> Vowels { get; private set; } = new HashSet<Phoneme>();
        public StressPattern StressPattern { get; private set; }
        public Researcher Creator { get; private set; }

        private readonly List<SoundShiftRule> _soundShiftRules = new List<SoundShiftRule>();
        public IReadOnlyCollection<SoundShiftRule> SoundShiftRules => _soundShiftRules.AsReadOnly();

        private readonly List<CognateSet> _cognateSets = new List<CognateSet>();
        public IReadOnlyCollection<CognateSet> CognateSets => _cognateSets.AsReadOnly();

        private PhonemeInventory() { }

        public PhonemeInventory(PhonemeInventoryId id, LanguageName name, StressPattern stressPattern, Researcher creator)
            : base(id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StressPattern = stressPattern;
            Creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }

        public void UpdateName(LanguageName name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public void UpdateStressPattern(StressPattern stressPattern)
        {
            StressPattern = stressPattern;
        }

        public void AddConsonant(Phoneme consonant)
        {
            if (consonant == null)
                throw new ArgumentNullException(nameof(consonant));

            if (consonant.IsVowel)
                throw new ArgumentException("Cannot add vowel as consonant", nameof(consonant));

            Consonants.Add(consonant);
        }

        public void AddVowel(Phoneme vowel)
        {
            if (vowel == null)
                throw new ArgumentNullException(nameof(vowel));

            if (!vowel.IsVowel)
                throw new ArgumentException("Cannot add consonant as vowel", nameof(vowel));

            Vowels.Add(vowel);
        }

        public void AddSoundShiftRule(SoundShiftRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _soundShiftRules.Add(rule);
        }

        public void AddCognateSet(CognateSet cognateSet)
        {
            if (cognateSet == null)
                throw new ArgumentNullException(nameof(cognateSet));

            _cognateSets.Add(cognateSet);
        }
    }
}
