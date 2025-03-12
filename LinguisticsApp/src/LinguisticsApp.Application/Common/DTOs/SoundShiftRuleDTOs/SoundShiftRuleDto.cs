using LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs;
using LinguisticsApp.Application.Common.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.SoundShiftRuleDTOs
{
    public class SoundShiftRuleDto : BaseDto
    {
        public string Type { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public PhonemeDto InputPhoneme { get; set; } = new PhonemeDto();
        public PhonemeDto OutputPhoneme { get; set; } = new PhonemeDto();
        public PhonemeInventorySummaryDto AppliesTo { get; set; } = new PhonemeInventorySummaryDto();
        public UserDto Creator { get; set; } = new UserDto();
    }
}
