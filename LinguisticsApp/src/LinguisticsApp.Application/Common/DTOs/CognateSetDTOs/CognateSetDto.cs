using LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.CognateSetDTOs
{
    public class CognateSetDto : BaseDto
    {
        public string ProtoForm { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public Dictionary<string, string> AttestedExamples { get; set; } = new Dictionary<string, string>();
        public PhonemeInventorySummaryDto ReconstructedFrom { get; set; } = new PhonemeInventorySummaryDto();
    }
}
