using LinguisticsApp.Application.Common.DTOs.CognateSetDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.SemanticShiftDTOs
{
    public class SemanticShiftDto : BaseDto
    {
        public string OriginalMeaning { get; set; } = string.Empty;
        public string ModernMeaning { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public CognateSetDto AffectedCognate { get; set; } = new CognateSetDto();
    }
}
