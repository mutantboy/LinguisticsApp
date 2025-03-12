using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.SemanticShiftCommands
{
    public class UpdateSemanticShiftCommand
    {
        public string? OriginalMeaning { get; set; }
        public string? ModernMeaning { get; set; }
        public ShiftTrigger? Trigger { get; set; }
    }
}
