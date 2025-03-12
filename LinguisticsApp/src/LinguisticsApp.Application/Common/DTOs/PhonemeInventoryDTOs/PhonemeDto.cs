using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs
{
    public class PhonemeDto
    {
        public string Value { get; set; } = string.Empty;
        public bool IsVowel { get; set; }
    }
}
