using LinguisticsApp.Application.Common.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs
{
    public class PhonemeInventoryDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public List<PhonemeDto> Consonants { get; set; } = new List<PhonemeDto>();
        public List<PhonemeDto> Vowels { get; set; } = new List<PhonemeDto>();
        public string StressPattern { get; set; } = string.Empty;
        public UserDto Creator { get; set; } = new UserDto();
    }
}
