using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.PhonemeInventoryCommands
{
    public class AddPhonemeCommand
    {
        [Required]
        public string Value { get; set; } = string.Empty;

        [Required]
        public bool IsVowel { get; set; }
    }
}
