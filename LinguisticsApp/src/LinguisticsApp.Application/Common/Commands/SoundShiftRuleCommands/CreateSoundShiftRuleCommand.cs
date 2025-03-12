using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.SoundShiftRuleCommands
{
    public class CreateSoundShiftRuleCommand
    {
        [Required]
        public RuleType Type { get; set; }

        [Required]
        public string Environment { get; set; } = string.Empty;

        [Required]
        public string InputPhonemeValue { get; set; } = string.Empty;

        [Required]
        public bool InputPhonemeIsVowel { get; set; }

        [Required]
        public string OutputPhonemeValue { get; set; } = string.Empty;

        [Required]
        public bool OutputPhonemeIsVowel { get; set; }

        [Required]
        public Guid AppliesToId { get; set; }
    }
}
