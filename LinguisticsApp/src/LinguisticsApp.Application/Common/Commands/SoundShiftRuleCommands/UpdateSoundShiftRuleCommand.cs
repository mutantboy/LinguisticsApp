using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.SoundShiftRuleCommands
{
    public class UpdateSoundShiftRuleCommand
    {
        public RuleType? Type { get; set; }
        public string? Environment { get; set; }
        public string? InputPhonemeValue { get; set; }
        public bool? InputPhonemeIsVowel { get; set; }
        public string? OutputPhonemeValue { get; set; }
        public bool? OutputPhonemeIsVowel { get; set; }
    }
}
