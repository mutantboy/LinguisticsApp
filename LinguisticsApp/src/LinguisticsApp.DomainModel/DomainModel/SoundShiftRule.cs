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
    public class SoundShiftRule : Entity<SoundShiftRuleId>
    {
        public RuleType Type { get; private set; }
        public string Environment { get; private set; }
        public Phoneme InputPhoneme { get; private set; }
        public Phoneme OutputPhoneme { get; private set; }
        public PhonemeInventory AppliesTo { get; private set; }
        public Researcher Creator { get; private set; }

        private SoundShiftRule() { }

        public SoundShiftRule(
            SoundShiftRuleId id,
            RuleType type,
            string environment,
            Phoneme inputPhoneme,
            Phoneme outputPhoneme,
            PhonemeInventory appliesTo,
            Researcher creator)
            : base(id)
        {
            Type = type;
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            InputPhoneme = inputPhoneme ?? throw new ArgumentNullException(nameof(inputPhoneme));
            OutputPhoneme = outputPhoneme ?? throw new ArgumentNullException(nameof(outputPhoneme));
            AppliesTo = appliesTo ?? throw new ArgumentNullException(nameof(appliesTo));
            Creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }

        public void UpdateRule(RuleType type, string environment, Phoneme inputPhoneme, Phoneme outputPhoneme)
        {
            Type = type;
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            InputPhoneme = inputPhoneme ?? throw new ArgumentNullException(nameof(inputPhoneme));
            OutputPhoneme = outputPhoneme ?? throw new ArgumentNullException(nameof(outputPhoneme));
        }
    }
}
