using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.SemanticShiftCommands
{
    public class CreateSemanticShiftCommand
    {
        [Required]
        public string OriginalMeaning { get; set; } = string.Empty;

        [Required]
        public string ModernMeaning { get; set; } = string.Empty;

        [Required]
        public ShiftTrigger Trigger { get; set; }

        [Required]
        public Guid AffectedCognateId { get; set; }
    }
}
