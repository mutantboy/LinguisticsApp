using LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.LanguageContactEventDTOs
{
    public class LanguageContactEventDto : BaseDto
    {
        public string Type { get; set; } = string.Empty;
        public List<string> LoanwordsAdopted { get; set; } = new List<string>();
        public string GrammaticalInfluence { get; set; } = string.Empty;
        public PhonemeInventorySummaryDto SourceLanguage { get; set; } = new PhonemeInventorySummaryDto();
        public PhonemeInventorySummaryDto TargetLanguage { get; set; } = new PhonemeInventorySummaryDto();
    }
}
