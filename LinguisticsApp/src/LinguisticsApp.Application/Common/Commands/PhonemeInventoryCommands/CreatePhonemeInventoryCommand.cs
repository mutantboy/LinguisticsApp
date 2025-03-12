using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.PhonemeInventoryCommands
{
    public class CreatePhonemeInventoryCommand
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public StressPattern StressPattern { get; set; }

        public List<PhonemeCommand> Consonants { get; set; } = new List<PhonemeCommand>();
        public List<PhonemeCommand> Vowels { get; set; } = new List<PhonemeCommand>();
    }
}
