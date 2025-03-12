using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.PhonemeInventoryCommands
{
    public class UpdatePhonemeInventoryCommand
    {
        public string? Name { get; set; }
        public StressPattern? StressPattern { get; set; }
    }
}
