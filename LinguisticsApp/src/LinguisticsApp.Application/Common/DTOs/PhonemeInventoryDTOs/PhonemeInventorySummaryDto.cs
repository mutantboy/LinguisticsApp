using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs
{
    public class PhonemeInventorySummaryDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public int ConsonantCount { get; set; }
        public int VowelCount { get; set; }
        public string StressPattern { get; set; } = string.Empty;
    }
}
